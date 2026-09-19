using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.Materials.Commands.ProcessMaterial;

/// <summary>
/// Pipeline da seção 7: extrai texto → normaliza → chunking (determinístico) → embedding local por
/// chunk (ONNX) → para cada tópico do edital, similaridade de cosseno máxima entre seus embeddings
/// e os chunks do material → grava MaterialTopic acima do threshold. RAG puro: nenhuma chamada de
/// LLM neste caminho.
/// </summary>
public class ProcessMaterialHandler(
    IStudyPlannerDbContext db,
    IFileStorage fileStorage,
    IPdfTextExtractor pdfTextExtractor,
    IEmbeddingGenerator embeddingGenerator) : IRequestHandler<ProcessMaterialCommand>
{
    /// <summary>
    /// Calibrado empiricamente: embeddings BERT-family têm similaridade "de base" alta entre
    /// frases quaisquer do mesmo idioma (anisotropia) — 0.35 deixava passar tópicos não
    /// relacionados. 0.5 separou corretamente tópicos cobertos (~0.53-0.55) de não cobertos
    /// (~0.45) num teste real. Ajustar se começar a gerar falsos negativos/positivos em produção.
    /// </summary>
    private const float RelevanceThreshold = 0.5f;

    public async Task Handle(ProcessMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await db.StudyMaterials.FirstOrDefaultAsync(m => m.Id == request.MaterialId, cancellationToken)
            ?? throw new KeyNotFoundException($"StudyMaterial {request.MaterialId} not found.");

        material.Status = StudyMaterialStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await using var fileStream = await fileStorage.OpenReadAsync(material.FileUrl, cancellationToken);
            var rawText = await pdfTextExtractor.ExtractTextAsync(fileStream, cancellationToken);
            var normalized = NoticeSectionSegmenter.Normalize(rawText);
            var chunkTexts = TextChunker.Chunk(normalized);

            if (chunkTexts.Count == 0)
            {
                throw new InvalidOperationException("Não foi possível extrair texto do material.");
            }

            var chunks = new List<MaterialChunk>();
            for (var i = 0; i < chunkTexts.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var embedding = await embeddingGenerator.GenerateAsync(chunkTexts[i], cancellationToken);
                var chunk = new MaterialChunk
                {
                    MaterialId = material.Id,
                    ChunkIndex = i,
                    Content = chunkTexts[i],
                    Embedding = embedding
                };
                chunks.Add(chunk);
                db.MaterialChunks.Add(chunk);
            }

            var subjects = await db.Subjects.Where(s => s.ExamId == material.ExamId).ToListAsync(cancellationToken);
            var subjectsById = subjects.ToDictionary(s => s.Id);
            var topics = await db.Topics
                .Where(t => subjects.Select(s => s.Id).Contains(t.SubjectId))
                .ToListAsync(cancellationToken);

            foreach (var topic in topics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var subjectName = subjectsById[topic.SubjectId].Name;
                var topicEmbedding = await embeddingGenerator.GenerateAsync($"{subjectName}: {topic.Name}", cancellationToken);

                var maxSimilarity = chunks.Max(c => VectorSimilarity.CosineSimilarity(c.Embedding, topicEmbedding));

                if (maxSimilarity >= RelevanceThreshold)
                {
                    db.MaterialTopics.Add(new MaterialTopic
                    {
                        MaterialId = material.Id,
                        TopicId = topic.Id,
                        RelevanceScore = (decimal)maxSimilarity
                    });
                }
            }

            material.Status = StudyMaterialStatus.Processed;
            material.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            material.Status = StudyMaterialStatus.Failed;
            material.ErrorMessage = ex.Message;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

using System.Text;
using MediatR;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Materials.Commands.ImportYouTubeMaterial;

/// <summary>Extrai a legenda do vídeo (IYouTubeTranscriptFetcher) e cria um StudyMaterial a partir dela — mesmo pipeline de processamento do upload de PDF a partir daqui.</summary>
public class ImportYouTubeMaterialHandler(
    IStudyPlannerDbContext db,
    IFileStorage fileStorage,
    IYouTubeTranscriptFetcher transcriptFetcher) : IRequestHandler<ImportYouTubeMaterialCommand, Guid>
{
    public async Task<Guid> Handle(ImportYouTubeMaterialCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var transcript = await transcriptFetcher.FetchAsync(request.YouTubeUrl, cancellationToken);

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(transcript.TranscriptText));
        var fileUrl = await fileStorage.SaveAsync(
            folder: $"materials/{request.ExamId}",
            fileName: "youtube-transcript.txt",
            content: contentStream,
            cancellationToken);

        var material = new StudyMaterial
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            Name = transcript.Title,
            FileUrl = fileUrl,
            SourceType = StudyMaterialSourceType.YouTube,
            SourceUrl = request.YouTubeUrl,
            Status = StudyMaterialStatus.Uploaded
        };

        db.StudyMaterials.Add(material);
        await db.SaveChangesAsync(cancellationToken);

        return material.Id;
    }
}

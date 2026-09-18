using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Application.Notices.Models;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Infrastructure.Llm;

/// <summary>
/// Adapter de ILlmClient que não chama nenhum provider externo — usa NumberedOutlineExtractor
/// (regex determinístico) para reconhecer editais com numeração hierárquica. Zero custo, zero
/// configuração. É um estágio intermediário até haver orçamento para um LLM real; troque para
/// AnthropicLlmClient (ou outro provider) em Program.cs quando fizer sentido.
/// </summary>
public class HeuristicExamStructureExtractor : ILlmClient
{
    public Task<ExtractedExamStructure> ExtractExamStructureAsync(string programContentText, CancellationToken cancellationToken)
    {
        var outline = NumberedOutlineExtractor.Extract(programContentText);

        if (outline.Count == 0)
        {
            throw new InvalidOperationException(
                "Não foi possível reconhecer a estrutura do edital automaticamente (o extrator heurístico só " +
                "entende numeração hierárquica do tipo \"1. Disciplina\" / \"1.1 Tópico\"). Cadastre manualmente " +
                "ou configure um provider de LLM real (Llm:Anthropic:ApiKey) para editais com formatação atípica.");
        }

        var subjects = outline
            .Select(subject => new ExtractedSubject(
                subject.Name,
                Weight: Math.Round(1m / outline.Count, 4),
                Topics: subject.Topics
                    .Select(topic => new ExtractedTopic(topic.Name, Math.Round(1m / subject.Topics.Count, 4)))
                    .ToList()))
            .ToList();

        return Task.FromResult(new ExtractedExamStructure(subjects));
    }
}

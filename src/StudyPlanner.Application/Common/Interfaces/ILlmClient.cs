using StudyPlanner.Application.Notices.Models;

namespace StudyPlanner.Application.Common.Interfaces;

/// <summary>
/// Único ponto de contato com o LLM na Application. Usado apenas onde há valor real:
/// interpretar texto livre do edital e devolver estrutura classificada. Nunca decide
/// prioridade, agenda ou domínio — isso é sempre PriorityEngine/MasteryCalculator.
/// </summary>
public interface ILlmClient
{
    Task<ExtractedExamStructure> ExtractExamStructureAsync(string programContentText, CancellationToken cancellationToken);
}

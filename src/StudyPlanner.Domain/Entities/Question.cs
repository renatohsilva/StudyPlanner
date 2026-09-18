using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class Question : Entity
{
    public required string Statement { get; set; }

    /// <summary>Alternativas em JSON (ex.: [{"key":"A","text":"..."}]). Nula para questões discursivas.</summary>
    public string? AlternativesJson { get; set; }

    public required string CorrectAnswer { get; set; }
    public required QuestionSource Source { get; set; }
    public Guid? BoardId { get; set; }
    public int? Year { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

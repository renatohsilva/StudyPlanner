using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class Subject : Entity
{
    public required Guid ExamId { get; set; }
    public required string Name { get; set; }

    /// <summary>Peso relativo da disciplina no edital, de 0 a 1. A soma das disciplinas de um mesmo edital deve ser 1.</summary>
    public decimal Weight { get; set; }

    public List<Topic> Topics { get; init; } = [];
}

using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class Topic : Entity
{
    public required Guid SubjectId { get; set; }
    public Guid? ParentTopicId { get; set; }
    public required string Name { get; set; }

    /// <summary>Incidência estimada do tópico dentro da disciplina, de 0 a 1 (quantidade de questões/relevância declarada no edital).</summary>
    public decimal EstimatedIncidence { get; set; }
}

namespace StudyPlanner.Domain.Entities;

/// <summary>Relação N:N entre Question e Topic. Chave composta (QuestionId, TopicId), configurada na Infrastructure.</summary>
public class QuestionTopic
{
    public required Guid QuestionId { get; set; }
    public required Guid TopicId { get; set; }
}

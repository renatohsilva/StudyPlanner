using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

/// <summary>Registro de exposição real a um tópico. Não altera TopicMastery por si só (ver TopicMastery).</summary>
public class StudySession : Entity
{
    public required Guid UserId { get; set; }
    public required Guid TopicId { get; set; }
    public Guid? StudyPlanItemId { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
}

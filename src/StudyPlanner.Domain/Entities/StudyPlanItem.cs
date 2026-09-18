using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class StudyPlanItem : Entity
{
    public required Guid StudyPlanId { get; set; }
    public required Guid TopicId { get; set; }
    public required StudyPlanItemType Type { get; set; }
    public required DateOnly ScheduledDate { get; set; }
    public int DurationMinutes { get; set; }

    /// <summary>Score calculado pelo PriorityEngine no momento da geração do plano — guardado para auditoria/explicação.</summary>
    public decimal PriorityScore { get; set; }
}

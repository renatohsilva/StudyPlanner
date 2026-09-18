using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class StudyPlan : Entity
{
    public required Guid UserId { get; set; }
    public required Guid ExamId { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public List<StudyPlanItem> Items { get; init; } = [];
}

namespace StudyPlanner.Application.StudyPlans.Queries.GetActiveWeeklyPlan;

public record WeeklyPlanItemDto(
    Guid Id,
    Guid TopicId,
    string TopicName,
    Guid SubjectId,
    string SubjectName,
    string Type,
    DateOnly ScheduledDate,
    int DurationMinutes,
    decimal PriorityScore);

public record WeeklyPlanDto(Guid? PlanId, int Version, IReadOnlyList<WeeklyPlanItemDto> Items);

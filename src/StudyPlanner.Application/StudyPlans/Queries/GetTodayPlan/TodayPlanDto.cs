namespace StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

public record TodayPlanDto(
    Guid ExamId,
    DateOnly Today,
    IReadOnlyList<TodayPlanItemDto> Items);

public record TodayPlanItemDto(
    Guid TopicId,
    string TopicName,
    Guid SubjectId,
    string SubjectName,
    decimal PriorityScore,
    decimal Mastery,
    int AttemptsCount,
    string SuggestedType);

namespace StudyPlanner.Application.Reviews.Queries.GetPendingReviews;

public record PendingReviewDto(
    Guid ReviewId,
    Guid TopicId,
    string TopicName,
    Guid SubjectId,
    string SubjectName,
    DateOnly ScheduledDate,
    int DaysOverdue,
    int Step);

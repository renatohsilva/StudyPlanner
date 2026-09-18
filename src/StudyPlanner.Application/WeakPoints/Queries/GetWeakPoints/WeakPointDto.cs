namespace StudyPlanner.Application.WeakPoints.Queries.GetWeakPoints;

public record WeakPointDto(
    Guid TopicId,
    string TopicName,
    Guid SubjectId,
    string SubjectName,
    decimal Mastery,
    int AttemptsCount,
    decimal WeakPointScore);

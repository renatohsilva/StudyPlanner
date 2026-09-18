using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class QuestionAttempt : Entity
{
    public required Guid UserId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string ChosenAnswer { get; set; }
    public required bool IsCorrect { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public DateTimeOffset AttemptedAt { get; init; } = DateTimeOffset.UtcNow;
}

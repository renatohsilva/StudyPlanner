using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class Exam : Entity
{
    public required Guid UserId { get; set; }
    public Guid? BoardId { get; set; }
    public required string Name { get; set; }
    public required DateOnly ExamDate { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public List<Subject> Subjects { get; init; } = [];
}

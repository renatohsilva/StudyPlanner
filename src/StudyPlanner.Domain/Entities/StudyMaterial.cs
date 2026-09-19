using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public enum StudyMaterialStatus
{
    Uploaded,
    Processing,
    Processed,
    Failed
}

public class StudyMaterial : Entity
{
    public required Guid UserId { get; set; }
    public required Guid ExamId { get; set; }
    public required string Name { get; set; }
    public required string FileUrl { get; set; }
    public StudyMaterialStatus Status { get; set; } = StudyMaterialStatus.Uploaded;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
}

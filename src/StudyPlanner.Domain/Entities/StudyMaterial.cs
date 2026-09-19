using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public enum StudyMaterialStatus
{
    Uploaded,
    Processing,
    Processed,
    Failed
}

/// <summary>De onde o texto do material veio. Todo tipo acaba salvo como texto plano via
/// IFileStorage (FileUrl) — o pipeline de chunking/embeddings não diferencia a origem depois disso.</summary>
public enum StudyMaterialSourceType
{
    File,
    YouTube,
    Link
}

public class StudyMaterial : Entity
{
    public required Guid UserId { get; set; }
    public required Guid ExamId { get; set; }
    public required string Name { get; set; }
    public required string FileUrl { get; set; }
    public StudyMaterialSourceType SourceType { get; set; } = StudyMaterialSourceType.File;

    /// <summary>URL original (YouTube/link), null para upload de arquivo.</summary>
    public string? SourceUrl { get; set; }

    public StudyMaterialStatus Status { get; set; } = StudyMaterialStatus.Uploaded;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
}

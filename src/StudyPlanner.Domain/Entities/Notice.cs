using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class Notice : Entity
{
    public required Guid ExamId { get; set; }
    public required string FileUrl { get; set; }
    public NoticeStatus Status { get; set; } = NoticeStatus.Uploaded;
    public int Version { get; set; } = 1;

    /// <summary>Estrutura extraída pelo LLM (JSON), pendente de confirmação do usuário. Nunca é persistida como Subject/Topic diretamente.</summary>
    public string? ExtractedStructureJson { get; set; }

    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}

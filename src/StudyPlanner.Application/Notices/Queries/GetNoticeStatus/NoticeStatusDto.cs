namespace StudyPlanner.Application.Notices.Queries.GetNoticeStatus;

public record NoticeStatusDto(
    Guid Id,
    Guid ExamId,
    string Status,
    string? ExtractedStructureJson,
    string? ErrorMessage);

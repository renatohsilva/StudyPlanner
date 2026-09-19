namespace StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

public record MaterialStatusDto(Guid Id, Guid ExamId, string Name, string Status, string? ErrorMessage);

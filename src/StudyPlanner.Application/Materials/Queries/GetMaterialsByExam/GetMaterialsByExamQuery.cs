using MediatR;
using StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialsByExam;

public record GetMaterialsByExamQuery(Guid ExamId) : IRequest<IReadOnlyList<MaterialStatusDto>>;

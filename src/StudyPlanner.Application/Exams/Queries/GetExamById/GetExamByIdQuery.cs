using MediatR;

namespace StudyPlanner.Application.Exams.Queries.GetExamById;

public record GetExamByIdQuery(Guid ExamId, Guid UserId) : IRequest<ExamDetailDto?>;

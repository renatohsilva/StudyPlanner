using MediatR;

namespace StudyPlanner.Application.Exams.Queries.GetExamById;

public record GetExamByIdQuery(Guid ExamId) : IRequest<ExamDetailDto?>;

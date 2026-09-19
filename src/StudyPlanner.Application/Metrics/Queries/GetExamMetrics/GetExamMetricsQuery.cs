using MediatR;

namespace StudyPlanner.Application.Metrics.Queries.GetExamMetrics;

public record GetExamMetricsQuery(Guid UserId, Guid ExamId) : IRequest<ExamMetricsDto>;

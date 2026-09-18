using MediatR;

namespace StudyPlanner.Application.WeakPoints.Queries.GetWeakPoints;

public record GetWeakPointsQuery(Guid UserId, Guid ExamId, int Top = 5) : IRequest<IReadOnlyList<WeakPointDto>>;

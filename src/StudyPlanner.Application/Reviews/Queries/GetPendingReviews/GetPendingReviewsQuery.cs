using MediatR;

namespace StudyPlanner.Application.Reviews.Queries.GetPendingReviews;

/// <summary>Revisões vencidas ou que vencem hoje (ScheduledDate &lt;= hoje), opcionalmente filtradas por exame.</summary>
public record GetPendingReviewsQuery(Guid UserId, Guid? ExamId = null) : IRequest<IReadOnlyList<PendingReviewDto>>;

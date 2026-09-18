using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Reviews.Queries.GetPendingReviews;

public class GetPendingReviewsHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetPendingReviewsQuery, IReadOnlyList<PendingReviewDto>>
{
    public async Task<IReadOnlyList<PendingReviewDto>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        var query =
            from review in db.Reviews
            where review.UserId == request.UserId
                  && review.Status == ReviewStatus.Pending
                  && review.ScheduledDate <= today
            join topic in db.Topics on review.TopicId equals topic.Id
            join subject in db.Subjects on topic.SubjectId equals subject.Id
            select new { review, topic, subject };

        if (request.ExamId is { } examId)
        {
            query = query.Where(x => x.subject.ExamId == examId);
        }

        var results = await query
            .OrderBy(x => x.review.ScheduledDate)
            .ToListAsync(cancellationToken);

        return results
            .Select(x => new PendingReviewDto(
                x.review.Id,
                x.topic.Id,
                x.topic.Name,
                x.subject.Id,
                x.subject.Name,
                x.review.ScheduledDate,
                Math.Max(0, today.DayNumber - x.review.ScheduledDate.DayNumber),
                x.review.Step))
            .ToList();
    }
}

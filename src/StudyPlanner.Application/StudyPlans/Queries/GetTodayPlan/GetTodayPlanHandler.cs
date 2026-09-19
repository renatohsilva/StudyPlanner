using MediatR;
using StudyPlanner.Application.StudyPlans;

namespace StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

public class GetTodayPlanHandler(RankedTopicsProvider rankedTopicsProvider) : IRequestHandler<GetTodayPlanQuery, TodayPlanDto>
{
    private const decimal ReviewMasteryThreshold = 0.5m;

    public async Task<TodayPlanDto> Handle(GetTodayPlanQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ranked = await rankedTopicsProvider.GetRankedTopicsAsync(request.UserId, request.ExamId, cancellationToken);

        var items = ranked
            .Take(request.Top)
            .Select(r => new TodayPlanItemDto(
                r.TopicId,
                r.TopicName,
                r.SubjectId,
                r.SubjectName,
                r.Score,
                r.Mastery,
                r.AttemptsCount,
                r.AttemptsCount > 0 && r.Mastery >= ReviewMasteryThreshold ? "Review" : "NewContent"))
            .ToList();

        return new TodayPlanDto(request.ExamId, today, items);
    }
}

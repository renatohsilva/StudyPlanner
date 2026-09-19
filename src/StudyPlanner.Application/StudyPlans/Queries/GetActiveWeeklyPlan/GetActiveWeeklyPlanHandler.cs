using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.StudyPlans.Queries.GetActiveWeeklyPlan;

public class GetActiveWeeklyPlanHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetActiveWeeklyPlanQuery, WeeklyPlanDto>
{
    public async Task<WeeklyPlanDto> Handle(GetActiveWeeklyPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await db.StudyPlans
            .Where(p => p.UserId == request.UserId && p.ExamId == request.ExamId && p.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan is null) return new WeeklyPlanDto(null, 0, []);

        var items = await (
                from item in db.StudyPlanItems
                where item.StudyPlanId == plan.Id
                join topic in db.Topics on item.TopicId equals topic.Id
                join subject in db.Subjects on topic.SubjectId equals subject.Id
                orderby item.ScheduledDate
                select new WeeklyPlanItemDto(
                    item.Id, topic.Id, topic.Name, subject.Id, subject.Name,
                    item.Type.ToString(), item.ScheduledDate, item.DurationMinutes, item.PriorityScore))
            .ToListAsync(cancellationToken);

        return new WeeklyPlanDto(plan.Id, plan.Version, items);
    }
}

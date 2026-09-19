using MediatR;

namespace StudyPlanner.Application.StudyPlans.Queries.GetActiveWeeklyPlan;

public record GetActiveWeeklyPlanQuery(Guid UserId, Guid ExamId) : IRequest<WeeklyPlanDto>;

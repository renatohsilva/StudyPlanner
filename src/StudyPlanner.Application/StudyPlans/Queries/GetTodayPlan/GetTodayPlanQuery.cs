using MediatR;

namespace StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

public record GetTodayPlanQuery(Guid UserId, Guid ExamId, int Top = 5) : IRequest<TodayPlanDto>;

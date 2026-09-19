using MediatR;

namespace StudyPlanner.Application.StudyPlans.Commands.GenerateWeeklyPlan;

public record GenerateWeeklyPlanCommand(Guid UserId, Guid ExamId, DateOnly WeekStartDate) : IRequest<Guid>;

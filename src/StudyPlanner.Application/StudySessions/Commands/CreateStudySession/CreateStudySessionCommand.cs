using MediatR;

namespace StudyPlanner.Application.StudySessions.Commands.CreateStudySession;

public record CreateStudySessionCommand(
    Guid UserId,
    Guid TopicId,
    Guid? StudyPlanItemId,
    int DurationMinutes) : IRequest<Guid>;

using MediatR;

namespace StudyPlanner.Application.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand(
    Guid UserId,
    Guid ExamId,
    string Name,
    decimal Weight) : IRequest<Guid>;

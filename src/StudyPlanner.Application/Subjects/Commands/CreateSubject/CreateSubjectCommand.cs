using MediatR;

namespace StudyPlanner.Application.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand(
    Guid ExamId,
    string Name,
    decimal Weight) : IRequest<Guid>;

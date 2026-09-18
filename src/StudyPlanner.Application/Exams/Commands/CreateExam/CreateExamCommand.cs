using MediatR;

namespace StudyPlanner.Application.Exams.Commands.CreateExam;

public record CreateExamCommand(
    Guid UserId,
    Guid? BoardId,
    string Name,
    DateOnly ExamDate) : IRequest<Guid>;

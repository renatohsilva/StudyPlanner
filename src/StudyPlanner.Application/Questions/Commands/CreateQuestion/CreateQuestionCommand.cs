using MediatR;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Questions.Commands.CreateQuestion;

public record CreateQuestionCommand(
    string Statement,
    string? AlternativesJson,
    string CorrectAnswer,
    QuestionSource Source,
    Guid? BoardId,
    int? Year,
    Guid? CreatedByUserId,
    IReadOnlyList<Guid> TopicIds) : IRequest<Guid>;

using MediatR;

namespace StudyPlanner.Application.Questions.Commands.AttemptQuestion;

public record AttemptQuestionCommand(
    Guid QuestionId,
    Guid UserId,
    string ChosenAnswer,
    int? TimeSpentSeconds) : IRequest<AttemptResultDto>;

public record AttemptResultDto(Guid AttemptId, bool IsCorrect, string CorrectAnswer);

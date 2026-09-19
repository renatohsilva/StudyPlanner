namespace StudyPlanner.Application.Questions.Queries.GetQuestionsByTopic;

public record QuestionSummaryDto(
    Guid Id,
    string Statement,
    string? AlternativesJson,
    string Source,
    int AttemptsCount);

using MediatR;

namespace StudyPlanner.Application.Questions.Queries.GetQuestionsByTopic;

public record GetQuestionsByTopicQuery(Guid TopicId, Guid UserId) : IRequest<IReadOnlyList<QuestionSummaryDto>>;

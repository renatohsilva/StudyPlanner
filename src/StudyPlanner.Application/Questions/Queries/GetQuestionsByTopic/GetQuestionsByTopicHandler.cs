using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Questions.Queries.GetQuestionsByTopic;

public class GetQuestionsByTopicHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetQuestionsByTopicQuery, IReadOnlyList<QuestionSummaryDto>>
{
    public async Task<IReadOnlyList<QuestionSummaryDto>> Handle(GetQuestionsByTopicQuery request, CancellationToken cancellationToken)
    {
        var questionIds = await db.QuestionTopics
            .Where(qt => qt.TopicId == request.TopicId)
            .Select(qt => qt.QuestionId)
            .ToListAsync(cancellationToken);

        var questions = await db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var attemptCounts = await db.QuestionAttempts
            .Where(a => a.UserId == request.UserId && questionIds.Contains(a.QuestionId))
            .GroupBy(a => a.QuestionId)
            .Select(g => new { QuestionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Count, cancellationToken);

        return questions
            .Select(q => new QuestionSummaryDto(
                q.Id,
                q.Statement,
                q.AlternativesJson,
                q.Source.ToString(),
                attemptCounts.GetValueOrDefault(q.Id, 0)))
            .OrderBy(q => q.AttemptsCount)
            .ToList();
    }
}

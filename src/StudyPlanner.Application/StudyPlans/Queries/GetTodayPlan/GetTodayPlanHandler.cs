using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

public class GetTodayPlanHandler(IStudyPlannerDbContext db) : IRequestHandler<GetTodayPlanQuery, TodayPlanDto>
{
    private const int RecentAttemptsWindow = 10;
    private const decimal ReviewMasteryThreshold = 0.5m;

    public async Task<TodayPlanDto> Handle(GetTodayPlanQuery request, CancellationToken cancellationToken)
    {
        var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam {request.ExamId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var subjects = await db.Subjects
            .Where(s => s.ExamId == request.ExamId)
            .ToListAsync(cancellationToken);
        var subjectsById = subjects.ToDictionary(s => s.Id);

        var subjectIds = subjects.Select(s => s.Id).ToList();
        var topics = await db.Topics
            .Where(t => subjectIds.Contains(t.SubjectId))
            .ToListAsync(cancellationToken);

        if (topics.Count == 0)
        {
            return new TodayPlanDto(request.ExamId, today, []);
        }

        var topicIds = topics.Select(t => t.Id).ToList();

        var masteries = await db.TopicMasteries
            .Where(tm => tm.UserId == request.UserId && topicIds.Contains(tm.TopicId))
            .ToDictionaryAsync(tm => tm.TopicId, cancellationToken);

        var recentAttemptsByTopic = (await db.QuestionAttempts
                .Where(qa => qa.UserId == request.UserId)
                .Join(db.QuestionTopics, qa => qa.QuestionId, qt => qt.QuestionId, (qa, qt) => new { qa.IsCorrect, qa.AttemptedAt, qt.TopicId })
                .Where(x => topicIds.Contains(x.TopicId))
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.TopicId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.AttemptedAt).Take(RecentAttemptsWindow).ToList());

        var priorityInputs = new List<TopicPriorityInput>(topics.Count);
        foreach (var topic in topics)
        {
            var subject = subjectsById[topic.SubjectId];
            var normalizedWeight = subject.Weight * topic.EstimatedIncidence;

            masteries.TryGetValue(topic.Id, out var mastery);
            var currentMastery = mastery?.Mastery ?? 0m;
            var attemptsCount = mastery?.AttemptsCount ?? 0;
            var lastStudiedAt = mastery?.LastUpdated;

            var recentErrorRate = 0m;
            if (recentAttemptsByTopic.TryGetValue(topic.Id, out var recent) && recent.Count > 0)
            {
                recentErrorRate = (decimal)recent.Count(a => !a.IsCorrect) / recent.Count;
            }

            priorityInputs.Add(new TopicPriorityInput(
                topic.Id, normalizedWeight, currentMastery, attemptsCount, lastStudiedAt, recentErrorRate));
        }

        var ranked = PriorityEngine.Rank(priorityInputs, exam.ExamDate, today);

        var inputsByTopicId = priorityInputs.ToDictionary(i => i.TopicId);
        var topicsById = topics.ToDictionary(t => t.Id);

        var items = ranked
            .Take(request.Top)
            .Select(r =>
            {
                var topic = topicsById[r.TopicId];
                var subject = subjectsById[topic.SubjectId];
                var input = inputsByTopicId[r.TopicId];
                var suggestedType = input.AttemptsCount > 0 && input.Mastery >= ReviewMasteryThreshold ? "Review" : "NewContent";

                return new TodayPlanItemDto(
                    topic.Id,
                    topic.Name,
                    subject.Id,
                    subject.Name,
                    r.Score,
                    input.Mastery,
                    input.AttemptsCount,
                    suggestedType);
            })
            .ToList();

        return new TodayPlanDto(request.ExamId, today, items);
    }
}

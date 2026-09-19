using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.StudyPlans;

public record RankedTopicResult(
    Guid TopicId,
    string TopicName,
    Guid SubjectId,
    string SubjectName,
    decimal Score,
    decimal Mastery,
    int AttemptsCount);

/// <summary>
/// Ranking de tópicos pelo PriorityEngine, compartilhado entre o plano de hoje e a geração do
/// plano semanal — mesma fonte de verdade, um único lugar que monta os TopicPriorityInput.
/// </summary>
public class RankedTopicsProvider(IStudyPlannerDbContext db)
{
    private const int RecentAttemptsWindow = 10;

    public async Task<IReadOnlyList<RankedTopicResult>> GetRankedTopicsAsync(
        Guid userId, Guid examId, CancellationToken cancellationToken)
    {
        var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == examId && e.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam {examId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var subjects = await db.Subjects.Where(s => s.ExamId == examId).ToListAsync(cancellationToken);
        var subjectsById = subjects.ToDictionary(s => s.Id);

        var subjectIds = subjects.Select(s => s.Id).ToList();
        var topics = await db.Topics.Where(t => subjectIds.Contains(t.SubjectId)).ToListAsync(cancellationToken);

        if (topics.Count == 0) return [];

        var topicIds = topics.Select(t => t.Id).ToList();

        var masteries = await db.TopicMasteries
            .Where(tm => tm.UserId == userId && topicIds.Contains(tm.TopicId))
            .ToDictionaryAsync(tm => tm.TopicId, cancellationToken);

        var recentAttemptsByTopic = (await db.QuestionAttempts
                .Where(qa => qa.UserId == userId)
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

        return ranked
            .Select(r =>
            {
                var topic = topicsById[r.TopicId];
                var subject = subjectsById[topic.SubjectId];
                var input = inputsByTopicId[r.TopicId];

                return new RankedTopicResult(topic.Id, topic.Name, subject.Id, subject.Name, r.Score, input.Mastery, input.AttemptsCount);
            })
            .ToList();
    }
}

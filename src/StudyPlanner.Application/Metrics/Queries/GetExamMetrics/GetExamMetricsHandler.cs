using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Metrics.Queries.GetExamMetrics;

/// <summary>
/// Agrega as métricas da seção 12 da análise. Todo cálculo é agregação determinística sobre dados
/// já persistidos (StudySession, QuestionAttempt, TopicMastery, Review) — sem LLM.
/// </summary>
public class GetExamMetricsHandler(IStudyPlannerDbContext db) : IRequestHandler<GetExamMetricsQuery, ExamMetricsDto>
{
    public async Task<ExamMetricsDto> Handle(GetExamMetricsQuery request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var subjects = await db.Subjects.Where(s => s.ExamId == request.ExamId).ToListAsync(cancellationToken);
        var subjectIds = subjects.Select(s => s.Id).ToList();

        var topics = await db.Topics.Where(t => subjectIds.Contains(t.SubjectId)).ToListAsync(cancellationToken);
        var topicIds = topics.Select(t => t.Id).ToList();

        var masteries = await db.TopicMasteries
            .Where(tm => tm.UserId == request.UserId && topicIds.Contains(tm.TopicId))
            .ToListAsync(cancellationToken);

        var sessions = await db.StudySessions
            .Where(s => s.UserId == request.UserId && topicIds.Contains(s.TopicId))
            .ToListAsync(cancellationToken);

        var attemptRows = await (
                from qa in db.QuestionAttempts
                where qa.UserId == request.UserId
                join qt in db.QuestionTopics on qa.QuestionId equals qt.QuestionId
                where topicIds.Contains(qt.TopicId)
                select new { qa.Id, qa.IsCorrect, qt.TopicId })
            .ToListAsync(cancellationToken);

        var reviews = await db.Reviews
            .Where(r => r.UserId == request.UserId && topicIds.Contains(r.TopicId))
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);

        var topicsStudied = sessions.Select(s => s.TopicId).Distinct().Count();
        var coverage = topics.Count == 0 ? 0m : (decimal)topicsStudied / topics.Count;

        var masteredTopics = masteries.Where(m => m.AttemptsCount > 0).ToList();
        var averageMastery = masteredTopics.Count == 0 ? 0m : masteredTopics.Average(m => m.Mastery);

        var studyHoursTotal = sessions.Sum(s => s.DurationMinutes) / 60m;
        var studyHoursLast7Days = sessions.Where(s => s.StartedAt >= sevenDaysAgo).Sum(s => s.DurationMinutes) / 60m;

        var distinctAttempts = attemptRows.GroupBy(a => a.Id).Select(g => g.First()).ToList();
        var questionsAnswered = distinctAttempts.Count;
        var accuracyOverall = questionsAnswered == 0 ? 0m : (decimal)distinctAttempts.Count(a => a.IsCorrect) / questionsAnswered;

        var completedReviews = reviews.Count(r => r.Status == ReviewStatus.Completed);
        var overduePendingReviews = reviews.Count(r => r.Status == ReviewStatus.Pending && r.ScheduledDate <= today);
        var revisionRate = (completedReviews + overduePendingReviews) == 0
            ? 0m
            : (decimal)completedReviews / (completedReviews + overduePendingReviews);

        var subjectMetrics = subjects.Select(subject =>
        {
            var subjectTopicIds = topics.Where(t => t.SubjectId == subject.Id).Select(t => t.Id).ToHashSet();

            var subjectMasteries = masteries.Where(m => subjectTopicIds.Contains(m.TopicId) && m.AttemptsCount > 0).ToList();
            var subjectAvgMastery = subjectMasteries.Count == 0 ? 0m : subjectMasteries.Average(m => m.Mastery);

            var subjectAttempts = attemptRows.Where(a => subjectTopicIds.Contains(a.TopicId))
                .GroupBy(a => a.Id).Select(g => g.First()).ToList();
            var subjectQuestionsAnswered = subjectAttempts.Count;
            var subjectAccuracy = subjectQuestionsAnswered == 0
                ? 0m
                : (decimal)subjectAttempts.Count(a => a.IsCorrect) / subjectQuestionsAnswered;

            return new SubjectMetricsDto(
                subject.Id,
                subject.Name,
                subject.Weight,
                subjectAvgMastery,
                subjectMasteries.Count,
                subjectTopicIds.Count,
                subjectQuestionsAnswered,
                subjectAccuracy);
        }).ToList();

        return new ExamMetricsDto(
            coverage,
            topics.Count,
            topicsStudied,
            averageMastery,
            masteredTopics.Count,
            studyHoursTotal,
            studyHoursLast7Days,
            questionsAnswered,
            accuracyOverall,
            revisionRate,
            subjectMetrics);
    }
}

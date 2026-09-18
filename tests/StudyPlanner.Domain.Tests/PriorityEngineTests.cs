using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class PriorityEngineTests
{
    private static readonly DateOnly Today = new(2026, 9, 17);
    private static readonly DateOnly ExamDate = Today.AddDays(60);

    [Fact]
    public void LowMasteryTopic_ScoresHigherThanHighMasteryTopic_WhenWeightIsEqual()
    {
        var weak = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.20m, AttemptsCount: 20, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.5m);
        var strong = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.90m, AttemptsCount: 20, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.05m);

        var ranked = PriorityEngine.Rank([weak, strong], ExamDate, Today);

        Assert.Equal(weak.TopicId, ranked[0].TopicId);
    }

    [Fact]
    public void HigherWeightTopic_ScoresHigherThanLowerWeightTopic_WhenMasteryIsEqual()
    {
        var highWeight = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.8m, Mastery: 0.5m, AttemptsCount: 10, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.3m);
        var lowWeight = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.1m, Mastery: 0.5m, AttemptsCount: 10, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.3m);

        var ranked = PriorityEngine.Rank([highWeight, lowWeight], ExamDate, Today);

        Assert.Equal(highWeight.TopicId, ranked[0].TopicId);
    }

    [Fact]
    public void StaleTopic_ScoresHigherThanRecentlyStudiedTopic_WhenOtherFactorsAreEqual()
    {
        var stale = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.6m, AttemptsCount: 15, LastStudiedAt: Today.AddDays(-60).ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.2m);
        var recent = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.6m, AttemptsCount: 15, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.2m);

        var ranked = PriorityEngine.Rank([stale, recent], ExamDate, Today);

        Assert.Equal(stale.TopicId, ranked[0].TopicId);
    }

    [Fact]
    public void NeverStudiedTopic_HasMaximumStaleness()
    {
        var neverStudied = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0m, AttemptsCount: 0, LastStudiedAt: null, RecentErrorRate: 0m);

        var staleness = PriorityEngine.CalculateStaleness(neverStudied.LastStudiedAt, DateTimeOffset.UtcNow);

        Assert.Equal(1m, staleness);
    }

    [Fact]
    public void LowConfidenceTopic_IsPenalizedLessThanHighConfidence_WhenBothLookWeak()
    {
        // Poucos dados (1 tentativa) não deveria gerar a mesma certeza de fraqueza que muitos dados (30 tentativas)
        // com a mesma taxa de acerto aparente — a penalidade de confiança deve reduzir o score do caso incerto.
        var fewAttempts = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.3m, AttemptsCount: 1, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.7m);
        var manyAttempts = new TopicPriorityInput(Guid.NewGuid(), NormalizedWeight: 0.5m, Mastery: 0.3m, AttemptsCount: 30, LastStudiedAt: Today.ToDateTime(TimeOnly.MinValue), RecentErrorRate: 0.7m);

        var urgency = PriorityEngine.CalculateUrgency(ExamDate, Today);
        var weights = new PriorityWeights();

        var fewScore = PriorityEngine.Calculate(fewAttempts, urgency, weights);
        var manyScore = PriorityEngine.Calculate(manyAttempts, urgency, weights);

        Assert.True(manyScore.Score > fewScore.Score);
    }

    [Fact]
    public void CalculateUrgency_IncreasesAsExamDateApproaches()
    {
        var urgencyFar = PriorityEngine.CalculateUrgency(Today.AddDays(150), Today, totalPlannedDays: 180);
        var urgencyNear = PriorityEngine.CalculateUrgency(Today.AddDays(10), Today, totalPlannedDays: 180);

        Assert.True(urgencyNear > urgencyFar);
    }

    [Fact]
    public void Rank_ReturnsResultsOrderedDescendingByScore()
    {
        var inputs = new[]
        {
            new TopicPriorityInput(Guid.NewGuid(), 0.2m, 0.9m, 20, Today.ToDateTime(TimeOnly.MinValue), 0.05m),
            new TopicPriorityInput(Guid.NewGuid(), 0.9m, 0.1m, 20, Today.AddDays(-40).ToDateTime(TimeOnly.MinValue), 0.6m),
            new TopicPriorityInput(Guid.NewGuid(), 0.5m, 0.5m, 20, Today.AddDays(-10).ToDateTime(TimeOnly.MinValue), 0.3m),
        };

        var ranked = PriorityEngine.Rank(inputs, ExamDate, Today);

        Assert.Equal(3, ranked.Count);
        Assert.True(ranked[0].Score >= ranked[1].Score);
        Assert.True(ranked[1].Score >= ranked[2].Score);
    }
}

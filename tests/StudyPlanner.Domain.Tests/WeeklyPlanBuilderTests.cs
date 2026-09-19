using StudyPlanner.Domain.Services;
using static StudyPlanner.Domain.Services.WeeklyPlanBuilder;

namespace StudyPlanner.Domain.Tests;

public class WeeklyPlanBuilderTests
{
    // 2026-09-21 é uma segunda-feira.
    private static readonly DateOnly Monday = new(2026, 9, 21);

    [Fact]
    public void Build_ReturnsEmpty_WhenNoTopics()
    {
        var result = Build([], [new DayAvailability(DayOfWeek.Monday, 2m)], Monday);

        Assert.Empty(result);
    }

    [Fact]
    public void Build_ReturnsEmpty_WhenNoAvailability()
    {
        var topics = new[] { new TopicInput(Guid.NewGuid(), 1m, 0m, 0) };

        var result = Build(topics, [], Monday);

        Assert.Empty(result);
    }

    [Fact]
    public void Build_AllocatesBlocks_OnlyOnDaysWithAvailability()
    {
        var topics = new[] { new TopicInput(Guid.NewGuid(), 1m, 0m, 0) };
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 2m) };

        var result = Build(topics, availability, Monday);

        Assert.All(result, b => Assert.Equal(DayOfWeek.Monday, b.Date.DayOfWeek));
    }

    [Fact]
    public void Build_FillsDayWithBlocksProportionalToHours()
    {
        var topics = new[] { new TopicInput(Guid.NewGuid(), 1m, 0m, 0) };
        // 2 horas = 120 min / 50 min por bloco = 2 blocos completos (resto descartado)
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 2m) };

        var result = Build(topics, availability, Monday, blockMinutes: 50);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Build_CyclesThroughRankedTopics_WhenMoreBlocksThanTopics()
    {
        var topicA = new TopicInput(Guid.NewGuid(), 1m, 0m, 0);
        var topicB = new TopicInput(Guid.NewGuid(), 0.5m, 0m, 0);
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 4m) }; // 4h / 50min ≈ 4 blocos

        var result = Build([topicA, topicB], availability, Monday, blockMinutes: 50);

        Assert.Equal(4, result.Count);
        Assert.Equal(topicA.TopicId, result[0].TopicId);
        Assert.Equal(topicB.TopicId, result[1].TopicId);
        Assert.Equal(topicA.TopicId, result[2].TopicId);
        Assert.Equal(topicB.TopicId, result[3].TopicId);
    }

    [Fact]
    public void Build_MarksAsReview_WhenTopicHasAttemptsAndHighMastery()
    {
        var reviewTopic = new TopicInput(Guid.NewGuid(), 1m, Mastery: 0.7m, AttemptsCount: 5);
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 1m) };

        var result = Build([reviewTopic], availability, Monday, blockMinutes: 50);

        Assert.Equal("Review", result[0].Type);
    }

    [Fact]
    public void Build_MarksAsNewContent_WhenTopicHasNoAttempts()
    {
        var newTopic = new TopicInput(Guid.NewGuid(), 1m, Mastery: 0m, AttemptsCount: 0);
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 1m) };

        var result = Build([newTopic], availability, Monday, blockMinutes: 50);

        Assert.Equal("NewContent", result[0].Type);
    }

    [Fact]
    public void Build_Throws_WhenBlockMinutesIsNotPositive()
    {
        var topics = new[] { new TopicInput(Guid.NewGuid(), 1m, 0m, 0) };
        var availability = new[] { new DayAvailability(DayOfWeek.Monday, 1m) };

        Assert.Throws<ArgumentOutOfRangeException>(() => Build(topics, availability, Monday, blockMinutes: 0));
    }
}

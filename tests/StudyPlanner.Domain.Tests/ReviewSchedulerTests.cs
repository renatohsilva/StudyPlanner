using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class ReviewSchedulerTests
{
    [Fact]
    public void NextStep_AdvancesOnSuccess()
    {
        Assert.Equal(1, ReviewScheduler.NextStep(0, wasSuccessful: true));
        Assert.Equal(2, ReviewScheduler.NextStep(1, wasSuccessful: true));
    }

    [Fact]
    public void NextStep_RegressesOnFailure()
    {
        Assert.Equal(1, ReviewScheduler.NextStep(2, wasSuccessful: false));
        Assert.Equal(0, ReviewScheduler.NextStep(0, wasSuccessful: false));
    }

    [Fact]
    public void NextStep_NeverExceedsMaxStep()
    {
        var atMax = ReviewScheduler.NextStep(ReviewScheduler.MaxStep, wasSuccessful: true);

        Assert.Equal(ReviewScheduler.MaxStep, atMax);
    }

    [Fact]
    public void NextStep_NeverGoesBelowZero()
    {
        var atMin = ReviewScheduler.NextStep(0, wasSuccessful: false);

        Assert.Equal(0, atMin);
    }

    [Fact]
    public void CalculateNextReviewDate_IsSoonerForLowerMastery()
    {
        var from = new DateOnly(2026, 9, 18);

        var lowMasteryDate = ReviewScheduler.CalculateNextReviewDate(step: 2, mastery: 0.1m, from);
        var highMasteryDate = ReviewScheduler.CalculateNextReviewDate(step: 2, mastery: 0.9m, from);

        Assert.True(lowMasteryDate < highMasteryDate);
    }

    [Fact]
    public void CalculateNextReviewDate_IsLaterForHigherStep_AtEqualMastery()
    {
        var from = new DateOnly(2026, 9, 18);

        var earlyStep = ReviewScheduler.CalculateNextReviewDate(step: 0, mastery: 0.7m, from);
        var laterStep = ReviewScheduler.CalculateNextReviewDate(step: 3, mastery: 0.7m, from);

        Assert.True(laterStep > earlyStep);
    }

    [Fact]
    public void CalculateNextReviewDate_NeverReturnsSameDayOrEarlier()
    {
        var from = new DateOnly(2026, 9, 18);

        var date = ReviewScheduler.CalculateNextReviewDate(step: 0, mastery: 0m, from);

        Assert.True(date > from);
    }
}

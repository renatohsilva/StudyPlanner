using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class MasteryCalculatorTests
{
    [Fact]
    public void CorrectAnswer_IncreasesMastery()
    {
        var updated = MasteryCalculator.UpdateMastery(currentMastery: 0.5m, isCorrect: true);

        Assert.True(updated > 0.5m);
    }

    [Fact]
    public void WrongAnswer_DecreasesMastery()
    {
        var updated = MasteryCalculator.UpdateMastery(currentMastery: 0.5m, isCorrect: false);

        Assert.True(updated < 0.5m);
    }

    [Fact]
    public void RecentAttempts_WeighMoreThanOlderOnes()
    {
        // Simula: começa em 0, acerta 3 vezes seguidas, depois erra uma — o erro recente
        // deve derrubar o mastery mais do que ele teria subido só com os 3 acertos antigos puros.
        var mastery = 0m;
        mastery = MasteryCalculator.UpdateMastery(mastery, true);
        mastery = MasteryCalculator.UpdateMastery(mastery, true);
        var afterThreeCorrect = MasteryCalculator.UpdateMastery(mastery, true);

        var afterOneWrong = MasteryCalculator.UpdateMastery(afterThreeCorrect, false);

        Assert.True(afterOneWrong < afterThreeCorrect);
    }

    [Fact]
    public void InvalidAlpha_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MasteryCalculator.UpdateMastery(0.5m, true, alpha: 1.5m));
    }
}

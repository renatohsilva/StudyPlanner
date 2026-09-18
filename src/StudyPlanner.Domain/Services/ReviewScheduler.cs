namespace StudyPlanner.Domain.Services;

/// <summary>
/// Agenda de revisão por tópico (seção 10 da análise): progressão de intervalos tipo Leitner/SM-2
/// simplificado, aplicada ao tópico agregado em vez de flashcards individuais. Determinístico —
/// sem LLM. Avança um passo quando a revisão é bem-sucedida, regride quando falha; o intervalo
/// também é ajustado pelo domínio atual do tópico (mais fraco → revisão mais próxima).
/// </summary>
public static class ReviewScheduler
{
    private static readonly int[] BaseIntervalDays = [3, 7, 14, 30, 60];

    public static int MaxStep => BaseIntervalDays.Length - 1;

    public static int NextStep(int currentStep, bool wasSuccessful)
    {
        var clamped = Math.Clamp(currentStep, 0, MaxStep);
        return wasSuccessful ? Math.Min(clamped + 1, MaxStep) : Math.Max(clamped - 1, 0);
    }

    /// <summary>0.5 (domínio 0, revisão mais próxima) a 1.0 (domínio 1, intervalo cheio).</summary>
    public static decimal CalculateDifficultyMultiplier(decimal mastery) =>
        Math.Clamp(0.5m + mastery * 0.5m, 0.5m, 1.0m);

    public static DateOnly CalculateNextReviewDate(int step, decimal mastery, DateOnly from)
    {
        var baseInterval = BaseIntervalDays[Math.Clamp(step, 0, MaxStep)];
        var multiplier = CalculateDifficultyMultiplier(mastery);
        var adjustedDays = Math.Max(1, (int)Math.Round(baseInterval * multiplier));
        return from.AddDays(adjustedDays);
    }
}

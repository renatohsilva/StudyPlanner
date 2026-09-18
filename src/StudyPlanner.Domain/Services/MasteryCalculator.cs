namespace StudyPlanner.Domain.Services;

/// <summary>
/// Atualiza o TopicMastery a partir de uma QuestionAttempt. Puro e determinístico — sem LLM.
/// A incerteza de amostras pequenas é tratada separadamente pelo PriorityEngine (via AttemptsCount),
/// não aqui — este serviço só calcula a média móvel exponencial do acerto.
/// </summary>
public static class MasteryCalculator
{
    public const decimal DefaultAlpha = 0.25m;

    public static decimal UpdateMastery(decimal currentMastery, bool isCorrect, decimal alpha = DefaultAlpha)
    {
        if (alpha is < 0m or > 1m) throw new ArgumentOutOfRangeException(nameof(alpha));

        var observed = isCorrect ? 1m : 0m;
        return alpha * observed + (1m - alpha) * currentMastery;
    }
}

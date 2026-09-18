namespace StudyPlanner.Domain.Services;

/// <summary>Pesos do PriorityScore. Configuráveis, mas sem LLM envolvido em nenhum termo.</summary>
public sealed record PriorityWeights(
    decimal WeightFactor = 0.30m,
    decimal MasteryFactor = 0.30m,
    decimal UrgencyFactor = 0.15m,
    decimal StalenessFactor = 0.15m,
    decimal ErrorRateFactor = 0.10m,
    decimal ConfidencePenalty = 0.10m);

/// <summary>Dados de um tópico necessários para calcular sua prioridade no plano.</summary>
public sealed record TopicPriorityInput(
    Guid TopicId,
    /// <summary>Peso da disciplina × incidência do tópico no edital, normalizado 0..1.</summary>
    decimal NormalizedWeight,
    /// <summary>TopicMastery.Mastery atual, 0..1.</summary>
    decimal Mastery,
    int AttemptsCount,
    DateTimeOffset? LastStudiedAt,
    /// <summary>Taxa de erro nas tentativas mais recentes (ex.: últimas 10), 0..1.</summary>
    decimal RecentErrorRate);

public sealed record TopicPriorityResult(Guid TopicId, decimal Score);

/// <summary>
/// Motor determinístico de priorização de tópicos (seção 2 da análise de produto).
/// Nenhum termo depende de LLM — apenas dados estruturados vindos de TopicMastery,
/// QuestionAttempt e do peso/incidência declarados no edital.
/// </summary>
public static class PriorityEngine
{
    private const int StalenessSaturationDays = 21;
    private const int ConfidenceFullAttempts = 10;

    public static decimal CalculateUrgency(DateOnly examDate, DateOnly today, int totalPlannedDays = 180)
    {
        if (totalPlannedDays <= 0) throw new ArgumentOutOfRangeException(nameof(totalPlannedDays));

        var daysRemaining = Math.Clamp(examDate.DayNumber - today.DayNumber, 0, totalPlannedDays);
        var urgency = 1m - (decimal)daysRemaining / totalPlannedDays;
        return Math.Clamp(urgency, 0m, 1m);
    }

    public static decimal CalculateStaleness(DateTimeOffset? lastStudiedAt, DateTimeOffset now)
    {
        if (lastStudiedAt is null) return 1m;

        var elapsedDays = (decimal)Math.Max(0, (now - lastStudiedAt.Value).TotalDays);
        return Math.Clamp(elapsedDays / StalenessSaturationDays, 0m, 1m);
    }

    public static decimal CalculateConfidence(int attemptsCount) =>
        Math.Clamp((decimal)attemptsCount / ConfidenceFullAttempts, 0m, 1m);

    public static TopicPriorityResult Calculate(TopicPriorityInput input, decimal urgency, PriorityWeights weights)
    {
        var confidence = CalculateConfidence(input.AttemptsCount);
        var staleness = CalculateStaleness(input.LastStudiedAt, DateTimeOffset.UtcNow);
        var weakness = 1m - input.Mastery;

        var score =
              weights.WeightFactor * input.NormalizedWeight
            + weights.MasteryFactor * weakness
            + weights.UrgencyFactor * urgency * input.NormalizedWeight
            + weights.StalenessFactor * staleness
            + weights.ErrorRateFactor * input.RecentErrorRate
            - weights.ConfidencePenalty * (1m - confidence) * weakness;

        return new TopicPriorityResult(input.TopicId, Math.Clamp(score, 0m, 10m));
    }

    /// <summary>Rankeia todos os tópicos de um exame, do maior para o menor PriorityScore.</summary>
    public static IReadOnlyList<TopicPriorityResult> Rank(
        IEnumerable<TopicPriorityInput> inputs,
        DateOnly examDate,
        DateOnly today,
        int totalPlannedDays = 180,
        PriorityWeights? weights = null)
    {
        weights ??= new PriorityWeights();
        var urgency = CalculateUrgency(examDate, today, totalPlannedDays);

        return inputs
            .Select(i => Calculate(i, urgency, weights))
            .OrderByDescending(r => r.Score)
            .ToList();
    }
}

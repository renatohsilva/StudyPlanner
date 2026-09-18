namespace StudyPlanner.Domain.Entities;

/// <summary>
/// Estado agregado e persistido de domínio de um tópico para um usuário.
/// Atualizado exclusivamente a partir de QuestionAttempt — nunca a partir de StudySession,
/// para manter a distinção entre "estudei" (exposição) e "domino" (evidência de desempenho).
/// Chave composta (UserId, TopicId), configurada na Infrastructure.
/// </summary>
public class TopicMastery
{
    public required Guid UserId { get; set; }
    public required Guid TopicId { get; set; }

    /// <summary>Média móvel exponencial ponderada por confiança (limite inferior de Wilson), 0 a 1.</summary>
    public decimal Mastery { get; set; }

    public int AttemptsCount { get; set; }
    public int CorrectCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

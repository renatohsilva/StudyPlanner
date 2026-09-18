using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

/// <summary>
/// Item de revisão agendado por tópico (não por questão individual). Cada linha representa uma
/// revisão pendente ou já resolvida; ao ser resolvida (via QuestionAttempt), gera a próxima linha —
/// isso preserva histórico para calcular RevisionRate depois. Ver ReviewScheduler para a lógica.
/// </summary>
public class Review : Entity
{
    public required Guid UserId { get; set; }
    public required Guid TopicId { get; set; }

    /// <summary>Índice na progressão de intervalos do ReviewScheduler (0 = mais curto).</summary>
    public int Step { get; set; }

    public required DateOnly ScheduledDate { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

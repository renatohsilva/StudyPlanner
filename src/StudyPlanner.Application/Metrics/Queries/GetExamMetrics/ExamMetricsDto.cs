namespace StudyPlanner.Application.Metrics.Queries.GetExamMetrics;

public record ExamMetricsDto(
    /// <summary>Tópicos com pelo menos uma StudySession / total de tópicos. Responde "quanto já foi estudado".</summary>
    decimal Coverage,
    int TotalTopics,
    int TopicsStudied,

    /// <summary>Média de TopicMastery.Mastery só entre tópicos com tentativas — "estudei" ≠ "domino".</summary>
    decimal AverageMastery,
    int TopicsWithAttempts,

    decimal StudyHoursTotal,
    decimal StudyHoursLast7Days,

    int QuestionsAnswered,
    decimal AccuracyOverall,

    /// <summary>Revisões concluídas / (concluídas + pendentes vencidas). Não penaliza revisões futuras ainda não vencidas.</summary>
    decimal RevisionRate,

    IReadOnlyList<SubjectMetricsDto> Subjects);

public record SubjectMetricsDto(
    Guid SubjectId,
    string SubjectName,
    decimal Weight,
    decimal AverageMastery,
    int TopicsWithAttempts,
    int TotalTopics,
    int QuestionsAnswered,
    decimal AccuracyOverall);

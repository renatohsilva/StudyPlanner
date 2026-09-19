namespace StudyPlanner.Domain.Services;

/// <summary>
/// Aloca os tópicos já ranqueados pelo PriorityEngine em blocos de tempo ao longo da semana,
/// respeitando a disponibilidade declarada pelo usuário (horas por dia da semana). Determinístico,
/// sem LLM — bin-packing simples: percorre a lista ranqueada ciclicamente, um bloco por vez, para
/// cada dia com disponibilidade, do mais prioritário para o menos.
/// </summary>
public static class WeeklyPlanBuilder
{
    public const int DefaultBlockMinutes = 50;
    private const decimal ReviewMasteryThreshold = 0.5m;

    public record TopicInput(Guid TopicId, decimal PriorityScore, decimal Mastery, int AttemptsCount);

    public record DayAvailability(DayOfWeek DayOfWeek, decimal HoursAvailable);

    public record PlannedBlock(Guid TopicId, DateOnly Date, string Type, int DurationMinutes, decimal PriorityScore);

    public static IReadOnlyList<PlannedBlock> Build(
        IReadOnlyList<TopicInput> rankedTopics,
        IReadOnlyList<DayAvailability> availability,
        DateOnly weekStartDate,
        int blockMinutes = DefaultBlockMinutes)
    {
        if (rankedTopics.Count == 0 || availability.Count == 0) return [];
        if (blockMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(blockMinutes));

        var availabilityByDay = availability.ToDictionary(a => a.DayOfWeek, a => a.HoursAvailable);
        var blocks = new List<PlannedBlock>();
        var topicIndex = 0;

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = weekStartDate.AddDays(dayOffset);
            if (!availabilityByDay.TryGetValue(date.DayOfWeek, out var hoursAvailable) || hoursAvailable <= 0) continue;

            var blocksForDay = (int)(hoursAvailable * 60 / blockMinutes);
            for (var b = 0; b < blocksForDay; b++)
            {
                var topic = rankedTopics[topicIndex % rankedTopics.Count];
                var type = topic.AttemptsCount > 0 && topic.Mastery >= ReviewMasteryThreshold ? "Review" : "NewContent";
                blocks.Add(new PlannedBlock(topic.TopicId, date, type, blockMinutes, topic.PriorityScore));
                topicIndex++;
            }
        }

        return blocks;
    }
}

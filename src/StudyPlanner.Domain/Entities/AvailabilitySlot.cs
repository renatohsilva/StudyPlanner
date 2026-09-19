using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

/// <summary>Horas disponíveis declaradas pelo usuário para um dia da semana, por concurso.</summary>
public class AvailabilitySlot : Entity
{
    public required Guid UserId { get; set; }
    public required Guid ExamId { get; set; }
    public required DayOfWeek DayOfWeek { get; set; }
    public required decimal HoursAvailable { get; set; }
}

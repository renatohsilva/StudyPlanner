using MediatR;

namespace StudyPlanner.Application.Availability.Commands.SetAvailability;

public record AvailabilitySlotInput(DayOfWeek DayOfWeek, decimal HoursAvailable);

/// <summary>Substitui toda a disponibilidade semanal declarada para este usuário/concurso.</summary>
public record SetAvailabilityCommand(Guid UserId, Guid ExamId, IReadOnlyList<AvailabilitySlotInput> Slots) : IRequest;

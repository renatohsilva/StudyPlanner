using MediatR;
using StudyPlanner.Application.Availability.Commands.SetAvailability;

namespace StudyPlanner.Application.Availability.Queries.GetAvailability;

public record GetAvailabilityQuery(Guid UserId, Guid ExamId) : IRequest<IReadOnlyList<AvailabilitySlotInput>>;

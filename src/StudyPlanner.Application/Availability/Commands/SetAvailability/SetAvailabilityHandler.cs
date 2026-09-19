using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Availability.Commands.SetAvailability;

public class SetAvailabilityHandler(IStudyPlannerDbContext db) : IRequestHandler<SetAvailabilityCommand>
{
    public async Task Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var existing = await db.AvailabilitySlots
            .Where(a => a.UserId == request.UserId && a.ExamId == request.ExamId)
            .ToListAsync(cancellationToken);
        db.AvailabilitySlots.RemoveRange(existing);

        foreach (var slot in request.Slots.Where(s => s.HoursAvailable > 0))
        {
            db.AvailabilitySlots.Add(new AvailabilitySlot
            {
                UserId = request.UserId,
                ExamId = request.ExamId,
                DayOfWeek = slot.DayOfWeek,
                HoursAvailable = slot.HoursAvailable
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

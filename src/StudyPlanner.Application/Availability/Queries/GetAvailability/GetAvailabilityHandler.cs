using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Availability.Commands.SetAvailability;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Availability.Queries.GetAvailability;

public class GetAvailabilityHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetAvailabilityQuery, IReadOnlyList<AvailabilitySlotInput>>
{
    public async Task<IReadOnlyList<AvailabilitySlotInput>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        return await db.AvailabilitySlots
            .Where(a => a.UserId == request.UserId && a.ExamId == request.ExamId)
            .Select(a => new AvailabilitySlotInput(a.DayOfWeek, a.HoursAvailable))
            .ToListAsync(cancellationToken);
    }
}

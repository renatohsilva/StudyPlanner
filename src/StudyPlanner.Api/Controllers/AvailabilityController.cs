using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Availability.Commands.SetAvailability;
using StudyPlanner.Application.Availability.Queries.GetAvailability;

namespace StudyPlanner.Api.Controllers;

[ApiController]
public class AvailabilityController(ISender sender) : ControllerBase
{
    public record SetAvailabilityRequest(IReadOnlyList<AvailabilitySlotInput> Slots);

    [HttpPost("api/exams/{examId:guid}/availability")]
    public async Task<IActionResult> Set(Guid examId, SetAvailabilityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new SetAvailabilityCommand(User.GetUserId(), examId, request.Slots), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("api/exams/{examId:guid}/availability")]
    public async Task<IActionResult> Get(Guid examId, CancellationToken cancellationToken)
    {
        var slots = await sender.Send(new GetAvailabilityQuery(User.GetUserId(), examId), cancellationToken);
        return Ok(slots);
    }
}

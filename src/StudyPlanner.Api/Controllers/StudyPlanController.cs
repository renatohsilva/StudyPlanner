using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/study-plan")]
public class StudyPlanController(ISender sender) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> Today(
        [FromQuery] Guid userId,
        [FromQuery] Guid examId,
        [FromQuery] int top,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = top > 0
                ? new GetTodayPlanQuery(userId, examId, top)
                : new GetTodayPlanQuery(userId, examId);

            var plan = await sender.Send(query, cancellationToken);
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

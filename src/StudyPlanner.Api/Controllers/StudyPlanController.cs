using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.StudyPlans.Commands.GenerateWeeklyPlan;
using StudyPlanner.Application.StudyPlans.Queries.GetActiveWeeklyPlan;
using StudyPlanner.Application.StudyPlans.Queries.GetTodayPlan;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/study-plan")]
public class StudyPlanController(ISender sender) : ControllerBase
{
    public record GenerateWeeklyPlanRequest(Guid UserId, DateOnly WeekStartDate);

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

    [HttpPost("~/api/exams/{examId:guid}/study-plan/generate")]
    public async Task<IActionResult> GenerateWeekly(Guid examId, GenerateWeeklyPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var planId = await sender.Send(
                new GenerateWeeklyPlanCommand(request.UserId, examId, request.WeekStartDate),
                cancellationToken);
            return Ok(new { id = planId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("~/api/exams/{examId:guid}/study-plan")]
    public async Task<IActionResult> GetWeekly(Guid examId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var plan = await sender.Send(new GetActiveWeeklyPlanQuery(userId, examId), cancellationToken);
        return Ok(plan);
    }
}

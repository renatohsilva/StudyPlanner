using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Exams.Commands.CreateExam;
using StudyPlanner.Application.Exams.Queries.GetExamById;
using StudyPlanner.Application.WeakPoints.Queries.GetWeakPoints;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/exams")]
public class ExamsController(ISender sender) : ControllerBase
{
    public record CreateExamRequest(Guid UserId, Guid? BoardId, string Name, DateOnly ExamDate);

    [HttpPost]
    public async Task<IActionResult> Create(CreateExamRequest request, CancellationToken cancellationToken)
    {
        var examId = await sender.Send(
            new CreateExamCommand(request.UserId, request.BoardId, request.Name, request.ExamDate),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = examId }, new { id = examId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var exam = await sender.Send(new GetExamByIdQuery(id), cancellationToken);
        return exam is null ? NotFound() : Ok(exam);
    }

    [HttpGet("{id:guid}/weak-points")]
    public async Task<IActionResult> WeakPoints(Guid id, [FromQuery] Guid userId, [FromQuery] int top, CancellationToken cancellationToken)
    {
        try
        {
            var query = top > 0 ? new GetWeakPointsQuery(userId, id, top) : new GetWeakPointsQuery(userId, id);
            var weakPoints = await sender.Send(query, cancellationToken);
            return Ok(weakPoints);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

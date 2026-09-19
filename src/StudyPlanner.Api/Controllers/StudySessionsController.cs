using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.StudySessions.Commands.CreateStudySession;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/study-sessions")]
public class StudySessionsController(ISender sender) : ControllerBase
{
    public record CreateStudySessionRequest(Guid TopicId, Guid? StudyPlanItemId, int DurationMinutes);

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudySessionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionId = await sender.Send(
                new CreateStudySessionCommand(User.GetUserId(), request.TopicId, request.StudyPlanItemId, request.DurationMinutes),
                cancellationToken);

            return CreatedAtAction(nameof(Create), new { id = sessionId }, new { id = sessionId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

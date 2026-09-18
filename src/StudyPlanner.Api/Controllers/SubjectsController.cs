using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Subjects.Commands.CreateSubject;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController(ISender sender) : ControllerBase
{
    public record CreateSubjectRequest(Guid ExamId, string Name, decimal Weight);

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var subjectId = await sender.Send(
                new CreateSubjectCommand(request.ExamId, request.Name, request.Weight),
                cancellationToken);

            return CreatedAtAction(nameof(Create), new { id = subjectId }, new { id = subjectId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

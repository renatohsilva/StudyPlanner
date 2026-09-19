using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Questions.Queries.GetQuestionsByTopic;
using StudyPlanner.Application.Topics.Commands.CreateTopic;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/topics")]
public class TopicsController(ISender sender) : ControllerBase
{
    public record CreateTopicRequest(Guid SubjectId, Guid? ParentTopicId, string Name, decimal EstimatedIncidence);

    [HttpPost]
    public async Task<IActionResult> Create(CreateTopicRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var topicId = await sender.Send(
                new CreateTopicCommand(User.GetUserId(), request.SubjectId, request.ParentTopicId, request.Name, request.EstimatedIncidence),
                cancellationToken);

            return CreatedAtAction(nameof(Create), new { id = topicId }, new { id = topicId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/questions")]
    public async Task<IActionResult> Questions(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var questions = await sender.Send(new GetQuestionsByTopicQuery(id, User.GetUserId()), cancellationToken);
            return Ok(questions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

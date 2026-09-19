using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Questions.Commands.AttemptQuestion;
using StudyPlanner.Application.Questions.Commands.CreateQuestion;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController(ISender sender) : ControllerBase
{
    public record CreateQuestionRequest(
        string Statement,
        string? AlternativesJson,
        string CorrectAnswer,
        QuestionSource Source,
        Guid? BoardId,
        int? Year,
        IReadOnlyList<Guid> TopicIds);

    public record AttemptQuestionRequest(string ChosenAnswer, int? TimeSpentSeconds);

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var questionId = await sender.Send(
            new CreateQuestionCommand(
                request.Statement,
                request.AlternativesJson,
                request.CorrectAnswer,
                request.Source,
                request.BoardId,
                request.Year,
                User.GetUserId(),
                request.TopicIds),
            cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = questionId }, new { id = questionId });
    }

    [HttpPost("{id:guid}/attempt")]
    public async Task<IActionResult> Attempt(Guid id, AttemptQuestionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(
                new AttemptQuestionCommand(id, User.GetUserId(), request.ChosenAnswer, request.TimeSpentSeconds),
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

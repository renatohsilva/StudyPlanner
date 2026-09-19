using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Notices.Commands.ConfirmNoticeStructure;
using StudyPlanner.Application.Notices.Commands.ProcessNotice;
using StudyPlanner.Application.Notices.Commands.UploadNotice;
using StudyPlanner.Application.Notices.Models;
using StudyPlanner.Application.Notices.Queries.GetNoticeStatus;

namespace StudyPlanner.Api.Controllers;

[ApiController]
public class NoticesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Faz upload do PDF do edital e dispara o pipeline de extração (texto → segmentação → LLM).
    /// Simplificação do MVP: processa de forma síncrona na própria requisição, em vez de enfileirar
    /// no Worker — aceitável para o volume inicial, mas o ponto de corte para mover ao Worker já
    /// está isolado em ProcessNoticeCommand.
    /// </summary>
    [HttpPost("api/exams/{examId:guid}/notice")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(Guid examId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) return BadRequest(new { message = "Arquivo vazio." });

        try
        {
            var userId = User.GetUserId();
            await using var stream = file.OpenReadStream();
            var noticeId = await sender.Send(new UploadNoticeCommand(userId, examId, file.FileName, stream), cancellationToken);

            await sender.Send(new ProcessNoticeCommand(noticeId), cancellationToken);

            var status = await sender.Send(new GetNoticeStatusQuery(noticeId, userId), cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("api/notices/{noticeId:guid}/status")]
    public async Task<IActionResult> Status(Guid noticeId, CancellationToken cancellationToken)
    {
        try
        {
            var status = await sender.Send(new GetNoticeStatusQuery(noticeId, User.GetUserId()), cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Materializa a estrutura (possivelmente editada pelo usuário) em Subject/Topic reais.</summary>
    [HttpPost("api/notices/{noticeId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid noticeId, ExtractedExamStructure structure, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new ConfirmNoticeStructureCommand(noticeId, User.GetUserId(), structure), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

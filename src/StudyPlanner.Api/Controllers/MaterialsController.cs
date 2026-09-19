using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Materials.Commands.ProcessMaterial;
using StudyPlanner.Application.Materials.Commands.UploadMaterial;
using StudyPlanner.Application.Materials.Queries.GetMaterialsByExam;
using StudyPlanner.Application.Materials.Queries.GetMaterialStatus;
using StudyPlanner.Application.Materials.Queries.GetMaterialTopics;

namespace StudyPlanner.Api.Controllers;

[ApiController]
public class MaterialsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Upload de material do aluno (apostila, resumo, PDF de aula). Processa de forma síncrona:
    /// extrai texto, gera chunks + embeddings locais (ONNX) e relaciona com os tópicos do edital.
    /// Mesma simplificação do pipeline de edital — pronto pra mover ao Worker quando o volume pedir.
    /// </summary>
    [HttpPost("api/exams/{examId:guid}/materials")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(Guid examId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) return BadRequest(new { message = "Arquivo vazio." });

        try
        {
            await using var stream = file.OpenReadStream();
            var materialId = await sender.Send(new UploadMaterialCommand(User.GetUserId(), examId, file.FileName, stream), cancellationToken);

            await sender.Send(new ProcessMaterialCommand(materialId), cancellationToken);

            var status = await sender.Send(new GetMaterialStatusQuery(materialId), cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("api/materials/{materialId:guid}/status")]
    public async Task<IActionResult> Status(Guid materialId, CancellationToken cancellationToken)
    {
        try
        {
            var status = await sender.Send(new GetMaterialStatusQuery(materialId), cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("api/materials/{materialId:guid}/topics")]
    public async Task<IActionResult> Topics(Guid materialId, CancellationToken cancellationToken)
    {
        var topics = await sender.Send(new GetMaterialTopicsQuery(materialId), cancellationToken);
        return Ok(topics);
    }

    [HttpGet("api/exams/{examId:guid}/materials")]
    public async Task<IActionResult> ByExam(Guid examId, CancellationToken cancellationToken)
    {
        var materials = await sender.Send(new GetMaterialsByExamQuery(examId), cancellationToken);
        return Ok(materials);
    }
}

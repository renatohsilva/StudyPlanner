using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.Notices.Commands.ProcessNotice;

/// <summary>
/// Orquestra o pipeline da seção 6: extração de texto → normalização/segmentação (determinístico)
/// → classificação por LLM → grava resultado como rascunho, pendente de confirmação do usuário.
/// Nunca escreve em Subject/Topic diretamente (ver ConfirmNoticeStructureHandler).
/// </summary>
public class ProcessNoticeHandler(
    IStudyPlannerDbContext db,
    IFileStorage fileStorage,
    IPdfTextExtractor pdfTextExtractor,
    ILlmClient llmClient) : IRequestHandler<ProcessNoticeCommand>
{
    public async Task Handle(ProcessNoticeCommand request, CancellationToken cancellationToken)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == request.NoticeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notice {request.NoticeId} not found.");

        notice.Status = NoticeStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await using var pdfStream = await fileStorage.OpenReadAsync(notice.FileUrl, cancellationToken);
            var rawText = await pdfTextExtractor.ExtractTextAsync(pdfStream, cancellationToken);

            var normalized = NoticeSectionSegmenter.Normalize(rawText);
            var programContent = NoticeSectionSegmenter.ExtractProgramContentSection(normalized);

            var structure = await llmClient.ExtractExamStructureAsync(programContent, cancellationToken);

            notice.ExtractedStructureJson = JsonSerializer.Serialize(structure);
            notice.Status = NoticeStatus.ExtractionReady;
            notice.ProcessedAt = DateTimeOffset.UtcNow;
            notice.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            notice.Status = NoticeStatus.Failed;
            notice.ErrorMessage = ex.Message;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

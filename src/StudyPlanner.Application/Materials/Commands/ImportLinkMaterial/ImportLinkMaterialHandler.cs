using System.Text;
using MediatR;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Materials.Commands.ImportLinkMaterial;

/// <summary>Extrai o texto legível de uma página (IWebPageTextExtractor) e cria um StudyMaterial a partir dela.</summary>
public class ImportLinkMaterialHandler(
    IStudyPlannerDbContext db,
    IFileStorage fileStorage,
    IWebPageTextExtractor webPageTextExtractor) : IRequestHandler<ImportLinkMaterialCommand, Guid>
{
    public async Task<Guid> Handle(ImportLinkMaterialCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var page = await webPageTextExtractor.ExtractAsync(request.Url, cancellationToken);

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(page.PageText));
        var fileUrl = await fileStorage.SaveAsync(
            folder: $"materials/{request.ExamId}",
            fileName: "link-content.txt",
            content: contentStream,
            cancellationToken);

        var material = new StudyMaterial
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            Name = page.Title,
            FileUrl = fileUrl,
            SourceType = StudyMaterialSourceType.Link,
            SourceUrl = request.Url,
            Status = StudyMaterialStatus.Uploaded
        };

        db.StudyMaterials.Add(material);
        await db.SaveChangesAsync(cancellationToken);

        return material.Id;
    }
}

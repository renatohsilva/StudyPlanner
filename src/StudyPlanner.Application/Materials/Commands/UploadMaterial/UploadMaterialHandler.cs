using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Materials.Commands.UploadMaterial;

public class UploadMaterialHandler(IStudyPlannerDbContext db, IFileStorage fileStorage)
    : IRequestHandler<UploadMaterialCommand, Guid>
{
    public async Task<Guid> Handle(UploadMaterialCommand request, CancellationToken cancellationToken)
    {
        var examExists = await db.Exams.AnyAsync(e => e.Id == request.ExamId, cancellationToken);
        if (!examExists) throw new KeyNotFoundException($"Exam {request.ExamId} not found.");

        var fileUrl = await fileStorage.SaveAsync(
            folder: $"materials/{request.ExamId}",
            fileName: request.FileName,
            content: request.Content,
            cancellationToken);

        var material = new StudyMaterial
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            Name = request.FileName,
            FileUrl = fileUrl,
            Status = StudyMaterialStatus.Uploaded
        };

        db.StudyMaterials.Add(material);
        await db.SaveChangesAsync(cancellationToken);

        return material.Id;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

public class GetMaterialStatusHandler(IStudyPlannerDbContext db) : IRequestHandler<GetMaterialStatusQuery, MaterialStatusDto>
{
    public async Task<MaterialStatusDto> Handle(GetMaterialStatusQuery request, CancellationToken cancellationToken)
    {
        var material = await db.StudyMaterials
                .FirstOrDefaultAsync(m => m.Id == request.MaterialId && m.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"StudyMaterial {request.MaterialId} not found.");

        return new MaterialStatusDto(material.Id, material.ExamId, material.Name, material.Status.ToString(), material.ErrorMessage);
    }
}

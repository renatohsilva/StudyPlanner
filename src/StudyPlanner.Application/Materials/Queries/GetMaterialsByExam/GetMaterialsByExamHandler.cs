using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialsByExam;

public class GetMaterialsByExamHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetMaterialsByExamQuery, IReadOnlyList<MaterialStatusDto>>
{
    public async Task<IReadOnlyList<MaterialStatusDto>> Handle(GetMaterialsByExamQuery request, CancellationToken cancellationToken)
    {
        return await db.StudyMaterials
            .Where(m => m.ExamId == request.ExamId)
            .OrderByDescending(m => m.UploadedAt)
            .Select(m => new MaterialStatusDto(m.Id, m.ExamId, m.Name, m.Status.ToString(), m.ErrorMessage))
            .ToListAsync(cancellationToken);
    }
}

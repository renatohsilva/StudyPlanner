using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialTopics;

public class GetMaterialTopicsHandler(IStudyPlannerDbContext db)
    : IRequestHandler<GetMaterialTopicsQuery, IReadOnlyList<MaterialTopicResultDto>>
{
    public async Task<IReadOnlyList<MaterialTopicResultDto>> Handle(GetMaterialTopicsQuery request, CancellationToken cancellationToken)
    {
        var materialOwned = await db.StudyMaterials
            .AnyAsync(m => m.Id == request.MaterialId && m.UserId == request.UserId, cancellationToken);
        if (!materialOwned) throw new KeyNotFoundException($"StudyMaterial {request.MaterialId} not found.");

        var results = await (
                from mt in db.MaterialTopics
                where mt.MaterialId == request.MaterialId
                join topic in db.Topics on mt.TopicId equals topic.Id
                join subject in db.Subjects on topic.SubjectId equals subject.Id
                orderby mt.RelevanceScore descending
                select new MaterialTopicResultDto(topic.Id, topic.Name, subject.Id, subject.Name, mt.RelevanceScore))
            .ToListAsync(cancellationToken);

        return results;
    }
}

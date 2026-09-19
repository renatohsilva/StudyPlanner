using MediatR;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialTopics;

public record GetMaterialTopicsQuery(Guid MaterialId) : IRequest<IReadOnlyList<MaterialTopicResultDto>>;

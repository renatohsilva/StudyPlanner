using MediatR;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

public record GetMaterialStatusQuery(Guid MaterialId, Guid UserId) : IRequest<MaterialStatusDto>;

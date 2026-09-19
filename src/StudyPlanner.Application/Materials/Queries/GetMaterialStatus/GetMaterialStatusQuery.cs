using MediatR;

namespace StudyPlanner.Application.Materials.Queries.GetMaterialStatus;

public record GetMaterialStatusQuery(Guid MaterialId) : IRequest<MaterialStatusDto>;

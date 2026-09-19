using MediatR;

namespace StudyPlanner.Application.Materials.Commands.ProcessMaterial;

public record ProcessMaterialCommand(Guid MaterialId) : IRequest;

using MediatR;

namespace StudyPlanner.Application.Materials.Commands.ImportLinkMaterial;

public record ImportLinkMaterialCommand(Guid UserId, Guid ExamId, string Url) : IRequest<Guid>;

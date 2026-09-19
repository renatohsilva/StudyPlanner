using MediatR;

namespace StudyPlanner.Application.Materials.Commands.UploadMaterial;

public record UploadMaterialCommand(Guid UserId, Guid ExamId, string FileName, Stream Content) : IRequest<Guid>;

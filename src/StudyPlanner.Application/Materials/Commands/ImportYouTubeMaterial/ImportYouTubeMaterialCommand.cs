using MediatR;

namespace StudyPlanner.Application.Materials.Commands.ImportYouTubeMaterial;

public record ImportYouTubeMaterialCommand(Guid UserId, Guid ExamId, string YouTubeUrl) : IRequest<Guid>;

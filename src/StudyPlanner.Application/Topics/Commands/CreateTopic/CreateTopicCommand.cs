using MediatR;

namespace StudyPlanner.Application.Topics.Commands.CreateTopic;

public record CreateTopicCommand(
    Guid UserId,
    Guid SubjectId,
    Guid? ParentTopicId,
    string Name,
    decimal EstimatedIncidence) : IRequest<Guid>;

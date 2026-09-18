using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Topics.Commands.CreateTopic;

public class CreateTopicHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateTopicCommand, Guid>
{
    public async Task<Guid> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        var subjectExists = await db.Subjects.AnyAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (!subjectExists) throw new KeyNotFoundException($"Subject {request.SubjectId} not found.");

        var topic = new Topic
        {
            SubjectId = request.SubjectId,
            ParentTopicId = request.ParentTopicId,
            Name = request.Name,
            EstimatedIncidence = request.EstimatedIncidence
        };

        db.Topics.Add(topic);
        await db.SaveChangesAsync(cancellationToken);

        return topic.Id;
    }
}

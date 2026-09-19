using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Topics.Commands.CreateTopic;

public class CreateTopicHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateTopicCommand, Guid>
{
    public async Task<Guid> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        var subjectOwned = await (
            from subject in db.Subjects
            join exam in db.Exams on subject.ExamId equals exam.Id
            where subject.Id == request.SubjectId && exam.UserId == request.UserId
            select subject.Id).AnyAsync(cancellationToken);
        if (!subjectOwned) throw new KeyNotFoundException($"Subject {request.SubjectId} not found.");

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

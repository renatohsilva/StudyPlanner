using MediatR;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Subjects.Commands.CreateSubject;

public class CreateSubjectHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateSubjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var subject = new Subject
        {
            ExamId = request.ExamId,
            Name = request.Name,
            Weight = request.Weight
        };

        db.Subjects.Add(subject);
        await db.SaveChangesAsync(cancellationToken);

        return subject.Id;
    }
}

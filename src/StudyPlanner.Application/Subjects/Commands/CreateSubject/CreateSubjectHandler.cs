using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Subjects.Commands.CreateSubject;

public class CreateSubjectHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateSubjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var examExists = await db.Exams.AnyAsync(e => e.Id == request.ExamId, cancellationToken);
        if (!examExists) throw new KeyNotFoundException($"Exam {request.ExamId} not found.");

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

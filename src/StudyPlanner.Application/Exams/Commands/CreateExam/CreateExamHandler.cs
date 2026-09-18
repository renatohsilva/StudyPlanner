using MediatR;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Exams.Commands.CreateExam;

public class CreateExamHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateExamCommand, Guid>
{
    public async Task<Guid> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var exam = new Exam
        {
            UserId = request.UserId,
            BoardId = request.BoardId,
            Name = request.Name,
            ExamDate = request.ExamDate
        };

        db.Exams.Add(exam);
        await db.SaveChangesAsync(cancellationToken);

        return exam.Id;
    }
}

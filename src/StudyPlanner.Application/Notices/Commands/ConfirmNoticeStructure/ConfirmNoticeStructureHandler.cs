using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Notices.Commands.ConfirmNoticeStructure;

public class ConfirmNoticeStructureHandler(IStudyPlannerDbContext db) : IRequestHandler<ConfirmNoticeStructureCommand>
{
    public async Task Handle(ConfirmNoticeStructureCommand request, CancellationToken cancellationToken)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == request.NoticeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notice {request.NoticeId} not found.");

        var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == notice.ExamId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exam {notice.ExamId} not found.");

        if (exam.UserId != request.UserId) throw new KeyNotFoundException($"Notice {request.NoticeId} not found.");

        foreach (var extractedSubject in request.Structure.Subjects)
        {
            var subject = new Subject
            {
                ExamId = exam.Id,
                Name = extractedSubject.Name,
                Weight = extractedSubject.Weight
            };
            db.Subjects.Add(subject);

            foreach (var extractedTopic in extractedSubject.Topics)
            {
                db.Topics.Add(new Topic
                {
                    SubjectId = subject.Id,
                    Name = extractedTopic.Name,
                    EstimatedIncidence = extractedTopic.EstimatedIncidence
                });
            }
        }

        notice.Status = NoticeStatus.Confirmed;
        exam.Status = ExamStatus.StructureConfirmed;

        await db.SaveChangesAsync(cancellationToken);
    }
}

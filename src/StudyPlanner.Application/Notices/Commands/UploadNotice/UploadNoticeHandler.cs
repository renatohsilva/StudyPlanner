using MediatR;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Notices.Commands.UploadNotice;

public class UploadNoticeHandler(IStudyPlannerDbContext db, IFileStorage fileStorage)
    : IRequestHandler<UploadNoticeCommand, Guid>
{
    public async Task<Guid> Handle(UploadNoticeCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var fileUrl = await fileStorage.SaveAsync(
            folder: $"notices/{request.ExamId}",
            fileName: request.FileName,
            content: request.Content,
            cancellationToken);

        var notice = new Notice
        {
            ExamId = request.ExamId,
            FileUrl = fileUrl,
            Status = NoticeStatus.Uploaded
        };

        db.Notices.Add(notice);
        await db.SaveChangesAsync(cancellationToken);

        return notice.Id;
    }
}

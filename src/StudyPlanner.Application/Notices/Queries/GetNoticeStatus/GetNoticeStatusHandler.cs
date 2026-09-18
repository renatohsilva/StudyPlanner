using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Notices.Queries.GetNoticeStatus;

public class GetNoticeStatusHandler(IStudyPlannerDbContext db) : IRequestHandler<GetNoticeStatusQuery, NoticeStatusDto>
{
    public async Task<NoticeStatusDto> Handle(GetNoticeStatusQuery request, CancellationToken cancellationToken)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == request.NoticeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notice {request.NoticeId} not found.");

        return new NoticeStatusDto(notice.Id, notice.ExamId, notice.Status.ToString(), notice.ExtractedStructureJson, notice.ErrorMessage);
    }
}

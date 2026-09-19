using MediatR;

namespace StudyPlanner.Application.Notices.Queries.GetNoticeStatus;

public record GetNoticeStatusQuery(Guid NoticeId, Guid UserId) : IRequest<NoticeStatusDto>;

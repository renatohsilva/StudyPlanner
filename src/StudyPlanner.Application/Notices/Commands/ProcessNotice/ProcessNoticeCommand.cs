using MediatR;

namespace StudyPlanner.Application.Notices.Commands.ProcessNotice;

public record ProcessNoticeCommand(Guid NoticeId) : IRequest;

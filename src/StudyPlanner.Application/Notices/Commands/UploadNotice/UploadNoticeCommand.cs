using MediatR;

namespace StudyPlanner.Application.Notices.Commands.UploadNotice;

public record UploadNoticeCommand(Guid UserId, Guid ExamId, string FileName, Stream Content) : IRequest<Guid>;

using MediatR;

namespace StudyPlanner.Application.Notices.Commands.UploadNotice;

public record UploadNoticeCommand(Guid ExamId, string FileName, Stream Content) : IRequest<Guid>;

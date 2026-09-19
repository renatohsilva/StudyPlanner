using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Exams.Queries.GetExamById;

public class GetExamByIdHandler(IStudyPlannerDbContext db) : IRequestHandler<GetExamByIdQuery, ExamDetailDto?>
{
    public async Task<ExamDetailDto?> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
    {
        var exam = await db.Exams
            .Where(e => e.Id == request.ExamId && e.UserId == request.UserId)
            .Select(e => new ExamDetailDto(
                e.Id,
                e.Name,
                e.ExamDate,
                e.Status.ToString(),
                e.Subjects.Select(s => new SubjectDto(
                    s.Id,
                    s.Name,
                    s.Weight,
                    s.Topics.Select(t => new TopicDto(t.Id, t.ParentTopicId, t.Name, t.EstimatedIncidence)).ToList()
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return exam;
    }
}

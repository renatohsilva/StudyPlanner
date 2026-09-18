using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.WeakPoints.Queries.GetWeakPoints;

/// <summary>
/// WeakPointScore = (1 - mastery) * peso_normalizado(tópico), só entre tópicos com evidência real
/// de desempenho (≥1 tentativa) — "nunca estudado" não é a mesma coisa que "ponto fraco".
/// </summary>
public class GetWeakPointsHandler(IStudyPlannerDbContext db) : IRequestHandler<GetWeakPointsQuery, IReadOnlyList<WeakPointDto>>
{
    public async Task<IReadOnlyList<WeakPointDto>> Handle(GetWeakPointsQuery request, CancellationToken cancellationToken)
    {
        var examExists = await db.Exams.AnyAsync(e => e.Id == request.ExamId, cancellationToken);
        if (!examExists) throw new KeyNotFoundException($"Exam {request.ExamId} not found.");

        var query =
            from mastery in db.TopicMasteries
            where mastery.UserId == request.UserId && mastery.AttemptsCount > 0
            join topic in db.Topics on mastery.TopicId equals topic.Id
            join subject in db.Subjects on topic.SubjectId equals subject.Id
            where subject.ExamId == request.ExamId
            select new
            {
                mastery,
                topic,
                subject,
                normalizedWeight = subject.Weight * topic.EstimatedIncidence
            };

        var candidates = await query.ToListAsync(cancellationToken);

        return candidates
            .Select(x => new WeakPointDto(
                x.topic.Id,
                x.topic.Name,
                x.subject.Id,
                x.subject.Name,
                x.mastery.Mastery,
                x.mastery.AttemptsCount,
                WeakPointScore: (1m - x.mastery.Mastery) * x.normalizedWeight))
            .OrderByDescending(w => w.WeakPointScore)
            .Take(request.Top)
            .ToList();
    }
}

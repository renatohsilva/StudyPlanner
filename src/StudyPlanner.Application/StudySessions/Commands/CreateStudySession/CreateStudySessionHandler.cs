using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.StudySessions.Commands.CreateStudySession;

/// <summary>
/// Registra apenas exposição ao tópico. Não altera TopicMastery — domínio só é atualizado
/// por evidência de desempenho (QuestionAttempt), nunca por tempo de estudo isolado.
/// </summary>
public class CreateStudySessionHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateStudySessionCommand, Guid>
{
    public async Task<Guid> Handle(CreateStudySessionCommand request, CancellationToken cancellationToken)
    {
        var topicExists = await db.Topics.AnyAsync(t => t.Id == request.TopicId, cancellationToken);
        if (!topicExists) throw new KeyNotFoundException($"Topic {request.TopicId} not found.");

        var session = new StudySession
        {
            UserId = request.UserId,
            TopicId = request.TopicId,
            StudyPlanItemId = request.StudyPlanItemId,
            DurationMinutes = request.DurationMinutes
        };

        db.StudySessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}

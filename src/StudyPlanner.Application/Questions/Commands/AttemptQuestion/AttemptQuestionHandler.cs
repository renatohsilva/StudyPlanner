using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.Questions.Commands.AttemptQuestion;

/// <summary>
/// Fecha o loop estudo → questão → desempenho: registra a tentativa e atualiza o TopicMastery
/// de cada tópico coberto pela questão. Cálculo 100% determinístico (MasteryCalculator), sem LLM.
/// </summary>
public class AttemptQuestionHandler(IStudyPlannerDbContext db) : IRequestHandler<AttemptQuestionCommand, AttemptResultDto>
{
    public async Task<AttemptResultDto> Handle(AttemptQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Question {request.QuestionId} not found.");

        var isCorrect = string.Equals(question.CorrectAnswer.Trim(), request.ChosenAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

        var attempt = new QuestionAttempt
        {
            UserId = request.UserId,
            QuestionId = request.QuestionId,
            ChosenAnswer = request.ChosenAnswer,
            IsCorrect = isCorrect,
            TimeSpentSeconds = request.TimeSpentSeconds
        };
        db.QuestionAttempts.Add(attempt);

        var topicIds = await db.QuestionTopics
            .Where(qt => qt.QuestionId == request.QuestionId)
            .Select(qt => qt.TopicId)
            .ToListAsync(cancellationToken);

        var existingMasteries = await db.TopicMasteries
            .Where(tm => tm.UserId == request.UserId && topicIds.Contains(tm.TopicId))
            .ToDictionaryAsync(tm => tm.TopicId, cancellationToken);

        foreach (var topicId in topicIds)
        {
            if (!existingMasteries.TryGetValue(topicId, out var mastery))
            {
                mastery = new TopicMastery { UserId = request.UserId, TopicId = topicId, Mastery = 0m };
                db.TopicMasteries.Add(mastery);
            }

            mastery.Mastery = MasteryCalculator.UpdateMastery(mastery.Mastery, isCorrect);
            mastery.AttemptsCount += 1;
            mastery.CorrectCount += isCorrect ? 1 : 0;
            mastery.LastUpdated = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new AttemptResultDto(attempt.Id, isCorrect, question.CorrectAnswer);
    }
}

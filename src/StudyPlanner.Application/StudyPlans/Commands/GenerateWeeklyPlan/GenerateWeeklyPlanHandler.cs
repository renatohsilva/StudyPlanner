using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Services;

namespace StudyPlanner.Application.StudyPlans.Commands.GenerateWeeklyPlan;

/// <summary>
/// Gera uma nova versão do plano semanal: rankeia tópicos (RankedTopicsProvider/PriorityEngine) e
/// aloca em blocos respeitando a disponibilidade declarada (WeeklyPlanBuilder). Versiona — nunca
/// sobrescreve o plano anterior, só desativa (StudyPlan.IsActive).
/// </summary>
public class GenerateWeeklyPlanHandler(IStudyPlannerDbContext db, RankedTopicsProvider rankedTopicsProvider)
    : IRequestHandler<GenerateWeeklyPlanCommand, Guid>
{
    public async Task<Guid> Handle(GenerateWeeklyPlanCommand request, CancellationToken cancellationToken)
    {
        await db.EnsureExamOwnedByAsync(request.ExamId, request.UserId, cancellationToken);

        var availabilitySlots = await db.AvailabilitySlots
            .Where(a => a.UserId == request.UserId && a.ExamId == request.ExamId)
            .ToListAsync(cancellationToken);

        if (availabilitySlots.Count == 0)
        {
            throw new InvalidOperationException(
                "Nenhuma disponibilidade configurada. Informe seus dias/horas de estudo antes de gerar o plano semanal.");
        }

        var rankedTopics = await rankedTopicsProvider.GetRankedTopicsAsync(request.UserId, request.ExamId, cancellationToken);
        if (rankedTopics.Count == 0)
        {
            throw new InvalidOperationException("Cadastre disciplinas e tópicos antes de gerar o plano semanal.");
        }

        var topicInputs = rankedTopics
            .Select(r => new WeeklyPlanBuilder.TopicInput(r.TopicId, r.Score, r.Mastery, r.AttemptsCount))
            .ToList();

        var dayAvailability = availabilitySlots
            .Select(a => new WeeklyPlanBuilder.DayAvailability(a.DayOfWeek, a.HoursAvailable))
            .ToList();

        var blocks = WeeklyPlanBuilder.Build(topicInputs, dayAvailability, request.WeekStartDate);

        var previousActivePlans = await db.StudyPlans
            .Where(p => p.UserId == request.UserId && p.ExamId == request.ExamId && p.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var previous in previousActivePlans) previous.IsActive = false;

        var lastVersion = await db.StudyPlans
            .Where(p => p.UserId == request.UserId && p.ExamId == request.ExamId)
            .Select(p => (int?)p.Version)
            .MaxAsync(cancellationToken) ?? 0;

        var plan = new StudyPlan
        {
            UserId = request.UserId,
            ExamId = request.ExamId,
            Version = lastVersion + 1,
            IsActive = true
        };
        db.StudyPlans.Add(plan);

        foreach (var block in blocks)
        {
            db.StudyPlanItems.Add(new StudyPlanItem
            {
                StudyPlanId = plan.Id,
                TopicId = block.TopicId,
                Type = Enum.Parse<StudyPlanItemType>(block.Type),
                ScheduledDate = block.Date,
                DurationMinutes = block.DurationMinutes,
                PriorityScore = block.PriorityScore
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}

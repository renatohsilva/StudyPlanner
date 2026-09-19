using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Application.Common;

/// <summary>
/// Autenticação garante quem está falando; isso garante que só é possível ler/escrever recursos do
/// próprio concurso. Lança KeyNotFoundException (não "Forbidden") de propósito — não confirma pra
/// quem não é dono que o concurso existe.
/// </summary>
public static class ExamOwnershipExtensions
{
    public static async Task EnsureExamOwnedByAsync(
        this IStudyPlannerDbContext db, Guid examId, Guid userId, CancellationToken cancellationToken)
    {
        var owned = await db.Exams.AnyAsync(e => e.Id == examId && e.UserId == userId, cancellationToken);
        if (!owned) throw new KeyNotFoundException($"Exam {examId} not found.");
    }
}

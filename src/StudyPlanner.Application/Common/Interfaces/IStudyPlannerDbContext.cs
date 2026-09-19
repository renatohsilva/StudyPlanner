using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Common.Interfaces;

/// <summary>
/// Abstração do DbContext exposta à Application, para que Commands/Queries não dependam
/// diretamente da Infrastructure (o EF Core concreto vive lá, implementando esta interface).
/// </summary>
public interface IStudyPlannerDbContext
{
    DbSet<User> Users { get; }
    DbSet<Board> Boards { get; }
    DbSet<Exam> Exams { get; }
    DbSet<Notice> Notices { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Topic> Topics { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionTopic> QuestionTopics { get; }
    DbSet<QuestionAttempt> QuestionAttempts { get; }
    DbSet<TopicMastery> TopicMasteries { get; }
    DbSet<StudyPlan> StudyPlans { get; }
    DbSet<StudyPlanItem> StudyPlanItems { get; }
    DbSet<StudySession> StudySessions { get; }
    DbSet<Review> Reviews { get; }
    DbSet<StudyMaterial> StudyMaterials { get; }
    DbSet<MaterialChunk> MaterialChunks { get; }
    DbSet<MaterialTopic> MaterialTopics { get; }
    DbSet<AvailabilitySlot> AvailabilitySlots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Auth;

namespace StudyPlanner.Infrastructure.Persistence;

public class StudyPlannerDbContext(DbContextOptions<StudyPlannerDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IStudyPlannerDbContext
{
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionTopic> QuestionTopics => Set<QuestionTopic>();
    public DbSet<QuestionAttempt> QuestionAttempts => Set<QuestionAttempt>();
    public DbSet<TopicMastery> TopicMasteries => Set<TopicMastery>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyPlanItem> StudyPlanItems => Set<StudyPlanItem>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
    public DbSet<MaterialChunk> MaterialChunks => Set<MaterialChunk>();
    public DbSet<MaterialTopic> MaterialTopics => Set<MaterialTopic>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyPlannerDbContext).Assembly);
    }
}

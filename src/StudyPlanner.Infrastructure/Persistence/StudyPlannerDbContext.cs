using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence;

public class StudyPlannerDbContext(DbContextOptions<StudyPlannerDbContext> options)
    : DbContext(options), IStudyPlannerDbContext
{
    public DbSet<User> Users => Set<User>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyPlannerDbContext).Assembly);
    }
}

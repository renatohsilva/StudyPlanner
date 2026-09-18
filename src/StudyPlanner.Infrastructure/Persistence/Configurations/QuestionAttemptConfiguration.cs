using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class QuestionAttemptConfiguration : IEntityTypeConfiguration<QuestionAttempt>
{
    public void Configure(EntityTypeBuilder<QuestionAttempt> builder)
    {
        builder.ToTable("question_attempts");
        builder.HasKey(qa => qa.Id);
        builder.Property(qa => qa.ChosenAnswer).IsRequired().HasMaxLength(32);

        builder.HasIndex(qa => new { qa.UserId, qa.QuestionId });
        builder.HasIndex(qa => new { qa.UserId, qa.AttemptedAt });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Statement).IsRequired();
        builder.Property(q => q.CorrectAnswer).IsRequired().HasMaxLength(32);
        builder.Property(q => q.Source).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(q => q.Source);
        builder.HasIndex(q => q.BoardId);
    }
}

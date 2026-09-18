using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class TopicMasteryConfiguration : IEntityTypeConfiguration<TopicMastery>
{
    public void Configure(EntityTypeBuilder<TopicMastery> builder)
    {
        builder.ToTable("topic_mastery");
        builder.HasKey(tm => new { tm.UserId, tm.TopicId });
        builder.Property(tm => tm.Mastery).HasPrecision(5, 4);
    }
}

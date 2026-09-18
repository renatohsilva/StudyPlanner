using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(r => new { r.UserId, r.ScheduledDate, r.Status });
        builder.HasIndex(r => new { r.UserId, r.TopicId });
    }
}

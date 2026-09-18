using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class StudyPlanItemConfiguration : IEntityTypeConfiguration<StudyPlanItem>
{
    public void Configure(EntityTypeBuilder<StudyPlanItem> builder)
    {
        builder.ToTable("study_plan_items");
        builder.HasKey(spi => spi.Id);
        builder.Property(spi => spi.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(spi => spi.PriorityScore).HasPrecision(6, 4);

        builder.HasIndex(spi => new { spi.StudyPlanId, spi.ScheduledDate });
    }
}

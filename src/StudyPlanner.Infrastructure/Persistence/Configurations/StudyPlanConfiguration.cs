using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> builder)
    {
        builder.ToTable("study_plans");
        builder.HasKey(sp => sp.Id);

        builder.HasMany(sp => sp.Items)
            .WithOne()
            .HasForeignKey(spi => spi.StudyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sp => new { sp.UserId, sp.ExamId, sp.IsActive });
    }
}

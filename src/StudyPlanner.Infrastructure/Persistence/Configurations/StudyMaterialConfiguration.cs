using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class StudyMaterialConfiguration : IEntityTypeConfiguration<StudyMaterial>
{
    public void Configure(EntityTypeBuilder<StudyMaterial> builder)
    {
        builder.ToTable("study_materials");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(256);
        builder.Property(m => m.FileUrl).IsRequired().HasMaxLength(1024);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(m => new { m.UserId, m.ExamId });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.ToTable("notices");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.FileUrl).IsRequired().HasMaxLength(1024);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(n => n.ExtractedStructureJson).HasColumnType("jsonb");

        builder.HasIndex(n => n.ExamId);
    }
}

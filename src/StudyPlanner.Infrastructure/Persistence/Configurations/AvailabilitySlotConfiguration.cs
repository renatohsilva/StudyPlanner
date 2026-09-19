using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("availability_slots");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.DayOfWeek).HasConversion<string>().HasMaxLength(16);
        builder.Property(a => a.HoursAvailable).HasPrecision(4, 2);

        builder.HasIndex(a => new { a.UserId, a.ExamId, a.DayOfWeek }).IsUnique();
    }
}

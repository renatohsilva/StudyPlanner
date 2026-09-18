using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(256);
        builder.Property(t => t.EstimatedIncidence).HasPrecision(5, 4);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(t => t.ParentTopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.SubjectId);
        builder.HasIndex(t => t.ParentTopicId);
    }
}

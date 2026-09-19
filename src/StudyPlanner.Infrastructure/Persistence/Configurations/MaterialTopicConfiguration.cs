using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class MaterialTopicConfiguration : IEntityTypeConfiguration<MaterialTopic>
{
    public void Configure(EntityTypeBuilder<MaterialTopic> builder)
    {
        builder.ToTable("material_topics");
        builder.HasKey(mt => new { mt.MaterialId, mt.TopicId });
        builder.Property(mt => mt.RelevanceScore).HasPrecision(5, 4);

        builder.HasIndex(mt => mt.TopicId);
    }
}

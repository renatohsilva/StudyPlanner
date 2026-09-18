using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class QuestionTopicConfiguration : IEntityTypeConfiguration<QuestionTopic>
{
    public void Configure(EntityTypeBuilder<QuestionTopic> builder)
    {
        builder.ToTable("question_topics");
        builder.HasKey(qt => new { qt.QuestionId, qt.TopicId });
        builder.HasIndex(qt => qt.TopicId);
    }
}

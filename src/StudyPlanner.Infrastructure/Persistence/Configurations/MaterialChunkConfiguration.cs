using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Persistence.Configurations;

public class MaterialChunkConfiguration : IEntityTypeConfiguration<MaterialChunk>
{
    /// <summary>Dimensão do all-MiniLM-L6-v2. Ver OnnxEmbeddingGenerator.</summary>
    public const int EmbeddingDimensions = 384;

    public void Configure(EntityTypeBuilder<MaterialChunk> builder)
    {
        builder.ToTable("material_chunks");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired();

        builder.Property(c => c.Embedding)
            .HasConversion(
                v => new Vector(v),
                v => v.ToArray(),
                new ValueComparer<float[]>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, x) => HashCode.Combine(hash, x)),
                    v => v.ToArray()))
            .HasColumnType($"vector({EmbeddingDimensions})");

        builder.HasIndex(c => c.MaterialId);
    }
}

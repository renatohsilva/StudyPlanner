using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

/// <summary>
/// Um pedaço de texto do material com seu embedding. O vetor em si é um detalhe de persistência
/// (pgvector); o Domain só conhece um float[] simples, sem depender de Npgsql/Pgvector.
/// </summary>
public class MaterialChunk : Entity
{
    public required Guid MaterialId { get; set; }
    public required int ChunkIndex { get; set; }
    public required string Content { get; set; }
    public required float[] Embedding { get; set; }
}

namespace StudyPlanner.Application.Common.Interfaces;

/// <summary>Gera embeddings semânticos de texto. Implementação local (ONNX), sem chamada externa.</summary>
public interface IEmbeddingGenerator
{
    Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken);
}

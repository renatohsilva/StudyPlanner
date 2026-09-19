using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.IntegrationTests.Fakes;

/// <summary>
/// Substitui o ONNX real nos testes de integração: determinístico (mesmo texto → mesmo vetor),
/// rápido, sem baixar/carregar um modelo de centenas de MB. Não tenta ser semanticamente
/// "inteligente" — só precisa ser estável o suficiente pra exercitar o pipeline (chunking →
/// embedding → similaridade → persistência) de ponta a ponta.
/// </summary>
public class FakeEmbeddingGenerator : IEmbeddingGenerator
{
    // Precisa bater com MaterialChunkConfiguration.EmbeddingDimensions (coluna vector(768) do pgvector).
    private const int Dimensions = 768;

    public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        var vector = new float[Dimensions];
        var hash = text.GetHashCode();
        var random = new Random(hash);

        for (var i = 0; i < Dimensions; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        }

        var norm = MathF.Sqrt(vector.Sum(v => v * v));
        if (norm > 0)
        {
            for (var i = 0; i < Dimensions; i++) vector[i] /= norm;
        }

        return Task.FromResult(vector);
    }
}

namespace StudyPlanner.Domain.Services;

/// <summary>Similaridade de cosseno entre embeddings — puro, sem dependência de pgvector/ONNX.</summary>
public static class VectorSimilarity
{
    public static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) throw new ArgumentException("Vetores devem ter o mesmo tamanho.");

        float dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0f;

        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}

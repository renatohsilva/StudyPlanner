using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class VectorSimilarityTests
{
    [Fact]
    public void CosineSimilarity_IsOne_ForIdenticalVectors()
    {
        float[] a = [1, 2, 3];

        var result = VectorSimilarity.CosineSimilarity(a, a);

        Assert.Equal(1f, result, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_IsZero_ForOrthogonalVectors()
    {
        float[] a = [1, 0];
        float[] b = [0, 1];

        var result = VectorSimilarity.CosineSimilarity(a, b);

        Assert.Equal(0f, result, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_IsNegative_ForOppositeVectors()
    {
        float[] a = [1, 0];
        float[] b = [-1, 0];

        var result = VectorSimilarity.CosineSimilarity(a, b);

        Assert.Equal(-1f, result, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_Throws_ForMismatchedLengths()
    {
        Assert.Throws<ArgumentException>(() => VectorSimilarity.CosineSimilarity([1, 2], [1, 2, 3]));
    }

    [Fact]
    public void CosineSimilarity_IsZero_ForZeroVector()
    {
        float[] a = [0, 0, 0];
        float[] b = [1, 2, 3];

        var result = VectorSimilarity.CosineSimilarity(a, b);

        Assert.Equal(0f, result);
    }
}

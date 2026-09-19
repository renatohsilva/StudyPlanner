using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class TextChunkerTests
{
    [Fact]
    public void Chunk_ReturnsEmpty_ForEmptyText()
    {
        Assert.Empty(TextChunker.Chunk(""));
    }

    [Fact]
    public void Chunk_ReturnsSingleChunk_WhenTextIsShorterThanTarget()
    {
        var text = "Direito Constitucional trata dos direitos fundamentais.";

        var chunks = TextChunker.Chunk(text, targetChars: 800, overlapChars: 150);

        Assert.Single(chunks);
        Assert.Equal(text, chunks[0]);
    }

    [Fact]
    public void Chunk_SplitsLongText_IntoMultipleChunks()
    {
        var text = string.Join(' ', Enumerable.Repeat("palavra", 500));

        var chunks = TextChunker.Chunk(text, targetChars: 800, overlapChars: 150);

        Assert.True(chunks.Count > 1);
    }

    [Fact]
    public void Chunk_ConsecutiveChunks_ShareOverlappingWords()
    {
        var text = string.Join(' ', Enumerable.Range(1, 500).Select(i => $"palavra{i}"));

        var chunks = TextChunker.Chunk(text, targetChars: 800, overlapChars: 150);

        Assert.True(chunks.Count > 1);
        var firstChunkWords = chunks[0].Split(' ');
        var secondChunkWords = chunks[1].Split(' ');

        // O primeiro chunk termina em uma palavra que também aparece perto do início do próximo —
        // confirma que a sobreposição realmente carrega contexto entre os chunks.
        Assert.Contains(firstChunkWords[^1], secondChunkWords);
    }

    [Fact]
    public void Chunk_Throws_WhenOverlapIsNotSmallerThanTarget()
    {
        Assert.Throws<ArgumentException>(() => TextChunker.Chunk("texto", targetChars: 100, overlapChars: 100));
    }
}

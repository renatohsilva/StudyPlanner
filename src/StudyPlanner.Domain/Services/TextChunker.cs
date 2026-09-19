namespace StudyPlanner.Domain.Services;

/// <summary>
/// Chunking determinístico de texto para RAG (seção 7 da análise): quebra por palavras respeitando
/// um tamanho-alvo em caracteres, com sobreposição entre chunks consecutivos para não perder
/// contexto na fronteira. Puro, sem I/O, sem LLM.
/// </summary>
public static class TextChunker
{
    public static IReadOnlyList<string> Chunk(string text, int targetChars = 800, int overlapChars = 150)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        if (overlapChars >= targetChars) throw new ArgumentException("overlapChars deve ser menor que targetChars.");

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var chunks = new List<string>();
        var current = new List<string>();
        var currentLength = 0;

        foreach (var word in words)
        {
            current.Add(word);
            currentLength += word.Length + 1;

            if (currentLength >= targetChars)
            {
                chunks.Add(string.Join(' ', current));
                current = TakeTrailingOverlap(current, overlapChars);
                currentLength = current.Sum(w => w.Length + 1);
            }
        }

        if (current.Count > 0 && (chunks.Count == 0 || !string.Equals(string.Join(' ', current), chunks[^1], StringComparison.Ordinal)))
        {
            chunks.Add(string.Join(' ', current));
        }

        return chunks;
    }

    private static List<string> TakeTrailingOverlap(List<string> words, int overlapChars)
    {
        var overlap = new List<string>();
        var length = 0;

        for (var i = words.Count - 1; i >= 0 && length < overlapChars; i--)
        {
            overlap.Insert(0, words[i]);
            length += words[i].Length + 1;
        }

        return overlap;
    }
}

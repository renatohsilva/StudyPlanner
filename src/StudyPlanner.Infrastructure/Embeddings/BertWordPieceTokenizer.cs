using System.Globalization;
using System.Text;

namespace StudyPlanner.Infrastructure.Embeddings;

/// <summary>
/// Tokenizador WordPiece (BERT-uncased) implementado à mão a partir do vocab.txt do modelo —
/// sem depender de nenhuma lib externa de tokenização. Segue o algoritmo padrão: lowercase,
/// remoção de acentos, split em pontuação, depois "maior substring conhecida primeiro" por palavra.
/// </summary>
public sealed class BertWordPieceTokenizer
{
    private const string UnkToken = "[UNK]";
    private const string ClsToken = "[CLS]";
    private const string SepToken = "[SEP]";
    private const string PadToken = "[PAD]";
    private const int MaxInputCharsPerWord = 200;

    private readonly Dictionary<string, int> _vocab;

    public BertWordPieceTokenizer(string vocabPath)
    {
        _vocab = File.ReadAllLines(vocabPath)
            .Select((line, index) => (line, index))
            .ToDictionary(x => x.line, x => x.index);
    }

    public (long[] InputIds, long[] AttentionMask, long[] TokenTypeIds) Tokenize(string text, int maxLength = 256)
    {
        var contentPieces = new List<string>();
        foreach (var word in BasicTokenize(text))
        {
            contentPieces.AddRange(WordPieceTokenize(word));
            if (contentPieces.Count >= maxLength - 2) break;
        }

        if (contentPieces.Count > maxLength - 2)
        {
            contentPieces = contentPieces.Take(maxLength - 2).ToList();
        }

        var tokens = new List<string> { ClsToken };
        tokens.AddRange(contentPieces);
        tokens.Add(SepToken);

        var inputIds = tokens.Select(t => (long)(_vocab.TryGetValue(t, out var id) ? id : _vocab[UnkToken])).ToList();
        var attentionMask = Enumerable.Repeat(1L, inputIds.Count).ToList();

        while (inputIds.Count < maxLength)
        {
            inputIds.Add(_vocab[PadToken]);
            attentionMask.Add(0L);
        }

        var tokenTypeIds = new long[maxLength];
        return (inputIds.ToArray(), attentionMask.ToArray(), tokenTypeIds);
    }

    private static List<string> BasicTokenize(string text)
    {
        var normalized = StripAccents(text.ToLowerInvariant());
        var tokens = new List<string>();
        var buffer = new StringBuilder();

        void Flush()
        {
            if (buffer.Length > 0)
            {
                tokens.Add(buffer.ToString());
                buffer.Clear();
            }
        }

        foreach (var c in normalized)
        {
            if (char.IsWhiteSpace(c))
            {
                Flush();
            }
            else if (char.IsPunctuation(c) || char.IsSymbol(c))
            {
                Flush();
                tokens.Add(c.ToString());
            }
            else
            {
                buffer.Append(c);
            }
        }

        Flush();
        return tokens;
    }

    private static string StripAccents(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private List<string> WordPieceTokenize(string word)
    {
        if (word.Length > MaxInputCharsPerWord) return [UnkToken];

        var subTokens = new List<string>();
        var start = 0;

        while (start < word.Length)
        {
            var end = word.Length;
            string? matched = null;

            while (start < end)
            {
                var substring = word[start..end];
                if (start > 0) substring = "##" + substring;

                if (_vocab.ContainsKey(substring))
                {
                    matched = substring;
                    break;
                }

                end--;
            }

            if (matched is null) return [UnkToken];

            subTokens.Add(matched);
            start = end;
        }

        return subTokens;
    }
}

using System.Text;
using System.Text.RegularExpressions;

namespace StudyPlanner.Domain.Services;

/// <summary>
/// Normalização e segmentação determinísticas de editais (seção 6 da análise de produto).
/// Nunca falha: se nenhuma âncora for encontrada, devolve o texto inteiro para o LLM classificar,
/// em vez de bloquear o pipeline. Puro — sem I/O, sem LLM, totalmente testável.
/// </summary>
public static class NoticeSectionSegmenter
{
    private static readonly string[] StartAnchors =
    [
        "CONTEÚDO PROGRAMÁTICO",
        "CONTEUDO PROGRAMATICO",
        "PROGRAMA DE PROVAS",
        "OBJETOS DE AVALIAÇÃO",
        "OBJETOS DE AVALIACAO"
    ];

    private static readonly string[] EndAnchors =
    [
        "ANEXO",
        "REFERÊNCIAS BIBLIOGRÁFICAS",
        "REFERENCIAS BIBLIOGRAFICAS",
        "DISPOSIÇÕES FINAIS",
        "DISPOSICOES FINAIS"
    ];

    /// <summary>Remove linhas repetidas (prováveis cabeçalhos/rodapés) e colapsa espaçamento excessivo.</summary>
    public static string Normalize(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        var lines = rawText.Replace("\r\n", "\n").Split('\n');

        var frequency = lines
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && l.Length <= 80)
            .GroupBy(l => l)
            .ToDictionary(g => g.Key, g => g.Count());

        const int repeatedLineThreshold = 3;

        var cleaned = lines
            .Where(line =>
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) return false;
                if (Regex.IsMatch(trimmed, @"^p[aá]gina\s+\d+(\s+de\s+\d+)?$", RegexOptions.IgnoreCase)) return false;
                if (frequency.TryGetValue(trimmed, out var count) && count >= repeatedLineThreshold) return false;
                return true;
            })
            .Select(l => l.Trim());

        var sb = new StringBuilder();
        foreach (var line in cleaned)
        {
            sb.AppendLine(line);
        }

        return Regex.Replace(sb.ToString(), @"\n{3,}", "\n\n").Trim();
    }

    /// <summary>Isola o trecho de conteúdo programático entre âncoras conhecidas, para reduzir ruído antes do LLM.</summary>
    public static string ExtractProgramContentSection(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText)) return string.Empty;

        var startIndex = FindFirstAnchorIndex(normalizedText, StartAnchors);
        if (startIndex is null) return normalizedText;

        var searchFrom = startIndex.Value;
        var endIndex = FindFirstAnchorIndex(normalizedText, EndAnchors, searchFrom + 1);

        return endIndex is null
            ? normalizedText[searchFrom..].Trim()
            : normalizedText[searchFrom..endIndex.Value].Trim();
    }

    private static int? FindFirstAnchorIndex(string text, IEnumerable<string> anchors, int startAt = 0)
    {
        int? best = null;
        foreach (var anchor in anchors)
        {
            var index = text.IndexOf(anchor, startAt, StringComparison.OrdinalIgnoreCase);
            if (index >= 0 && (best is null || index < best))
            {
                best = index;
            }
        }

        return best;
    }
}

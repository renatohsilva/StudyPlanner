using System.Text.RegularExpressions;

namespace StudyPlanner.Domain.Services;

/// <summary>
/// Fallback determinístico (sem LLM) para reconhecer a estrutura de um edital quando ele segue
/// numeração hierárquica comum ("1. DISCIPLINA" / "1.1 tópico"). Não substitui a classificação por
/// LLM em qualidade — é um extrator honesto: só funciona com editais numerados, e não tenta
/// adivinhar estrutura em formatos atípicos (ALL CAPS solto, bullets, tabelas). Puro e testável.
/// </summary>
public static class NumberedOutlineExtractor
{
    public record OutlineTopic(string Name);
    public record OutlineSubject(string Name, IReadOnlyList<OutlineTopic> Topics);

    // "1. DIREITO CONSTITUCIONAL" — um número, ponto, espaço, texto. Não deve casar com "1.1 ...".
    private static readonly Regex SubjectPattern = new(@"^(\d+)\.\s+(.+)$", RegexOptions.Compiled);

    // "1.1 Direitos fundamentais" ou "1.1. Direitos fundamentais"
    private static readonly Regex TopicPattern = new(@"^(\d+)\.(\d+)\.?\s+(.+)$", RegexOptions.Compiled);

    public static IReadOnlyList<OutlineSubject> Extract(string text)
    {
        var subjects = new List<(string Name, List<OutlineTopic> Topics)>();
        (string Name, List<OutlineTopic> Topics)? current = null;

        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            var topicMatch = TopicPattern.Match(line);
            if (topicMatch.Success && current is not null)
            {
                current.Value.Topics.Add(new OutlineTopic(topicMatch.Groups[3].Value.Trim()));
                continue;
            }

            var subjectMatch = SubjectPattern.Match(line);
            if (subjectMatch.Success)
            {
                current = (subjectMatch.Groups[2].Value.Trim(), []);
                subjects.Add(current.Value);
            }
        }

        return subjects
            .Where(s => s.Topics.Count > 0)
            .Select(s => new OutlineSubject(s.Name, s.Topics))
            .ToList();
    }
}

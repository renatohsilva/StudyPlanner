namespace StudyPlanner.Application.Common.Interfaces;

public record WebPageContent(string Title, string PageText);

/// <summary>Baixa uma página e extrai o texto legível (sem nav/script/style) — sem LLM.</summary>
public interface IWebPageTextExtractor
{
    Task<WebPageContent> ExtractAsync(string url, CancellationToken cancellationToken);
}

using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.IntegrationTests.Fakes;

/// <summary>Evita depender de rede real nos testes — devolve um texto de página fixo.</summary>
public class FakeWebPageTextExtractor : IWebPageTextExtractor
{
    public Task<WebPageContent> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        return Task.FromResult(new WebPageContent(
            Title: "Página de teste",
            PageText: "Este é um texto de página fake para testes de integração."));
    }
}

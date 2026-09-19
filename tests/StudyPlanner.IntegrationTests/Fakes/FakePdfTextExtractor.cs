using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.IntegrationTests.Fakes;

/// <summary>Ignora o binário e devolve o conteúdo como texto puro — evita depender de PdfPig/PDFs reais nos testes.</summary>
public class FakePdfTextExtractor : IPdfTextExtractor
{
    public async Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(pdfContent);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}

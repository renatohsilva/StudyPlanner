namespace StudyPlanner.Application.Common.Interfaces;

/// <summary>Extração determinística de texto bruto de um PDF — sem LLM.</summary>
public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken);
}

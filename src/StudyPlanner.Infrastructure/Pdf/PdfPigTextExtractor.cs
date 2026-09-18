using System.Text;
using StudyPlanner.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace StudyPlanner.Infrastructure.Pdf;

public class PdfPigTextExtractor : IPdfTextExtractor
{
    public Task<string> ExtractTextAsync(Stream pdfContent, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(pdfContent);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(page.Text);
        }

        return Task.FromResult(sb.ToString());
    }
}

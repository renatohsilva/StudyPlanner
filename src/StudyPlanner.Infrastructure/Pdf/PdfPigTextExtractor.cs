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

            // page.Text concatena palavras sem preservar quebras de linha entre blocos de texto
            // visualmente separados. Reconstituímos linhas agrupando palavras pela posição Y da
            // base (baseline) e ordenando por X dentro de cada linha — essencial para o
            // NumberedOutlineExtractor, que depende de padrões por linha ("1. Disciplina").
            var lines = page.GetWords()
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
        }

        return Task.FromResult(sb.ToString());
    }
}

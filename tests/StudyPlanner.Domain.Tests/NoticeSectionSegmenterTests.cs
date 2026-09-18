using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class NoticeSectionSegmenterTests
{
    [Fact]
    public void Normalize_RemovesRepeatedHeaderAndFooterLines()
    {
        var raw = string.Join('\n',
        [
            "EDITAL Nº 01/2026 - TRT-SP",
            "1. DAS DISPOSIÇÕES PRELIMINARES",
            "texto da disposição",
            "EDITAL Nº 01/2026 - TRT-SP",
            "2. DOS REQUISITOS",
            "texto dos requisitos",
            "EDITAL Nº 01/2026 - TRT-SP",
            "Página 1 de 40",
            "3. FIM"
        ]);

        var normalized = NoticeSectionSegmenter.Normalize(raw);

        Assert.DoesNotContain("EDITAL Nº 01/2026 - TRT-SP", normalized);
        Assert.DoesNotContain("Página 1 de 40", normalized);
        Assert.Contains("texto da disposição", normalized);
        Assert.Contains("texto dos requisitos", normalized);
    }

    [Fact]
    public void Normalize_CollapsesExcessiveBlankLines()
    {
        var raw = "Linha 1\n\n\n\n\nLinha 2";

        var normalized = NoticeSectionSegmenter.Normalize(raw);

        Assert.DoesNotContain("\n\n\n", normalized);
    }

    [Fact]
    public void ExtractProgramContentSection_IsolatesTextBetweenKnownAnchors()
    {
        var text = "PREÂMBULO irrelevante\n\nCONTEÚDO PROGRAMÁTICO\nDireito Constitucional: direitos fundamentais\n\nANEXO I - MODELO DE RECURSO\ntexto irrelevante de anexo";

        var section = NoticeSectionSegmenter.ExtractProgramContentSection(text);

        Assert.Contains("Direito Constitucional", section);
        Assert.DoesNotContain("PREÂMBULO", section);
        Assert.DoesNotContain("MODELO DE RECURSO", section);
    }

    [Fact]
    public void ExtractProgramContentSection_FallsBackToFullText_WhenNoAnchorFound()
    {
        var text = "Um edital com formatação atípica que não usa os termos esperados.";

        var section = NoticeSectionSegmenter.ExtractProgramContentSection(text);

        Assert.Equal(text, section);
    }

    [Fact]
    public void ExtractProgramContentSection_ReturnsToEndOfText_WhenNoEndAnchorFound()
    {
        var text = "PREÂMBULO\n\nCONTEÚDO PROGRAMÁTICO\nDireito Administrativo: licitações e contratos";

        var section = NoticeSectionSegmenter.ExtractProgramContentSection(text);

        Assert.Contains("licitações e contratos", section);
        Assert.DoesNotContain("PREÂMBULO", section);
    }
}

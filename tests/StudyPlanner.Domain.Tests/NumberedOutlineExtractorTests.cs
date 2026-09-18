using StudyPlanner.Domain.Services;

namespace StudyPlanner.Domain.Tests;

public class NumberedOutlineExtractorTests
{
    [Fact]
    public void Extract_RecognizesSubjectsAndTopics_FromNumberedOutline()
    {
        var text = string.Join('\n',
        [
            "1. DIREITO CONSTITUCIONAL",
            "1.1 Direitos e garantias fundamentais",
            "1.2 Organizacao do Estado",
            "2. DIREITO ADMINISTRATIVO",
            "2.1 Licitacoes e contratos administrativos",
            "2.2 Atos administrativos"
        ]);

        var result = NumberedOutlineExtractor.Extract(text);

        Assert.Equal(2, result.Count);
        Assert.Equal("DIREITO CONSTITUCIONAL", result[0].Name);
        Assert.Equal(2, result[0].Topics.Count);
        Assert.Equal("Direitos e garantias fundamentais", result[0].Topics[0].Name);
        Assert.Equal("DIREITO ADMINISTRATIVO", result[1].Name);
        Assert.Equal("Licitacoes e contratos administrativos", result[1].Topics[0].Name);
    }

    [Fact]
    public void Extract_SupportsTopicPattern_WithTrailingDot()
    {
        var text = "1. DIREITO CONSTITUCIONAL\n1.1. Direitos fundamentais";

        var result = NumberedOutlineExtractor.Extract(text);

        Assert.Single(result);
        Assert.Single(result[0].Topics);
        Assert.Equal("Direitos fundamentais", result[0].Topics[0].Name);
    }

    [Fact]
    public void Extract_IgnoresSubjectsWithoutAnyTopics()
    {
        var text = "1. DISCIPLINA SEM TOPICOS\n2. DIREITO ADMINISTRATIVO\n2.1 Licitacoes";

        var result = NumberedOutlineExtractor.Extract(text);

        Assert.Single(result);
        Assert.Equal("DIREITO ADMINISTRATIVO", result[0].Name);
    }

    [Fact]
    public void Extract_ReturnsEmpty_WhenTextHasNoNumberedOutline()
    {
        var text = "Um edital com formatação totalmente atípica, sem numeração nenhuma.";

        var result = NumberedOutlineExtractor.Extract(text);

        Assert.Empty(result);
    }

    [Fact]
    public void Extract_IgnoresTopicLines_BeforeAnySubjectIsFound()
    {
        var text = "1.1 topico orfao antes de qualquer disciplina\n1. DIREITO CONSTITUCIONAL\n1.1 Direitos fundamentais";

        var result = NumberedOutlineExtractor.Extract(text);

        Assert.Single(result);
        Assert.Single(result[0].Topics);
    }
}

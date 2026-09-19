using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Infrastructure.ExternalContent;

/// <summary>
/// Baixa uma página e extrai o texto legível: remove script/style/nav/header/footer, prefere
/// &lt;article&gt;/&lt;main&gt; quando existem. Heurística simples — não é um "Readability" completo,
/// mas cobre bem artigos/posts de blog, que é o caso de uso principal (material de estudo em texto).
/// </summary>
public class HtmlAgilityWebPageTextExtractor(HttpClient httpClient) : IWebPageTextExtractor
{
    private static readonly string[] RemovableTags = ["script", "style", "nav", "header", "footer", "aside", "noscript", "iframe", "form"];
    private static readonly Regex ExcessiveWhitespace = new(@"[ \t]+", RegexOptions.Compiled);
    private static readonly Regex ExcessiveBlankLines = new(@"\n{3,}", RegexOptions.Compiled);

    public async Task<WebPageContent> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; StudyPlannerBot/1.0)");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Não foi possível acessar o link ({(int)response.StatusCode}).");
        }

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim() ?? url;

        foreach (var tag in RemovableTags)
        {
            foreach (var node in doc.DocumentNode.SelectNodes($"//{tag}")?.ToList() ?? [])
            {
                node.Remove();
            }
        }

        var contentRoot =
            doc.DocumentNode.SelectSingleNode("//article")
            ?? doc.DocumentNode.SelectSingleNode("//main")
            ?? doc.DocumentNode.SelectSingleNode("//body")
            ?? doc.DocumentNode;

        var text = ExtractVisibleText(contentRoot);
        text = ExcessiveWhitespace.Replace(text, " ");
        text = ExcessiveBlankLines.Replace(text, "\n\n").Trim();

        if (text.Length == 0)
        {
            throw new InvalidOperationException("Não foi possível extrair texto legível dessa página.");
        }

        return new WebPageContent(title, text);
    }

    private static string ExtractVisibleText(HtmlNode root)
    {
        var sb = new StringBuilder();
        CollectText(root, sb);
        return sb.ToString();
    }

    private static void CollectText(HtmlNode node, StringBuilder sb)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
            if (text.Length > 0) sb.Append(text).Append(' ');
            return;
        }

        foreach (var child in node.ChildNodes)
        {
            CollectText(child, sb);
        }

        if (node.Name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
        {
            sb.Append('\n');
        }
    }
}

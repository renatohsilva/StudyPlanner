using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Infrastructure.ExternalContent;

/// <summary>
/// Extrai a legenda de um vídeo do YouTube chamando o executável yt-dlp (precisa estar instalado —
/// ver README). Scraping direto via HTTP (sem yt-dlp) parou de funcionar: o YouTube passou a exigir
/// um token de sessão amarrado a um carregamento real de página, que uma requisição HTTP simples não
/// tem como fornecer — yt-dlp é mantido ativamente contra esse tipo de bloqueio.
/// </summary>
public class YtDlpTranscriptFetcher(IOptions<YtDlpOptions> options) : IYouTubeTranscriptFetcher
{
    private static readonly Regex TimestampLine = new(@"-->", RegexOptions.Compiled);
    private static readonly Regex CueNumberLine = new(@"^\d+$", RegexOptions.Compiled);
    private static readonly Regex InlineTag = new(@"<[^>]+>", RegexOptions.Compiled);

    private readonly YtDlpOptions _options = options.Value;

    public async Task<YouTubeTranscript> FetchAsync(string youTubeUrl, CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"yt-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Duas chamadas separadas de propósito: --print faz o yt-dlp entrar em modo
            // "simulação" que pula a etapa de baixar legenda — combinar as duas na mesma
            // invocação silenciosamente não gera nenhum arquivo .vtt.
            var title = await GetTitleAsync(youTubeUrl, cancellationToken);
            await DownloadSubtitlesAsync(youTubeUrl, tempDir, cancellationToken);

            var vttFiles = Directory.GetFiles(tempDir, "*.vtt");
            if (vttFiles.Length == 0)
            {
                throw new InvalidOperationException(
                    "Este vídeo não tem legenda disponível (manual ou automática) nos idiomas configurados.");
            }

            var preferredLanguage = _options.SubtitleLanguages.Split(',')[0].Trim();
            var chosenFile = vttFiles.FirstOrDefault(f => f.Contains($".{preferredLanguage}.", StringComparison.OrdinalIgnoreCase))
                ?? vttFiles[0];

            var vttContent = await File.ReadAllTextAsync(chosenFile, cancellationToken);
            var transcriptText = ParseVtt(vttContent);

            return new YouTubeTranscript(title, transcriptText);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best effort cleanup */ }
        }
    }

    private async Task<string> GetTitleAsync(string youTubeUrl, CancellationToken cancellationToken)
    {
        var (exitCode, stdout, _) = await RunAsync(["--skip-download", "--print", "%(title)s", youTubeUrl], cancellationToken);
        if (exitCode != 0) return youTubeUrl;

        return stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? youTubeUrl;
    }

    private async Task DownloadSubtitlesAsync(string youTubeUrl, string outputDir, CancellationToken cancellationToken)
    {
        var outputTemplate = Path.Combine(outputDir, "sub");
        var (exitCode, _, stderr) = await RunAsync(
            [
                "--skip-download", "--write-auto-sub", "--write-sub",
                "--sub-lang", _options.SubtitleLanguages,
                "--sub-format", "vtt",
                "-o", outputTemplate,
                youTubeUrl
            ],
            cancellationToken);

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"yt-dlp não encontrado ou falhou (código {exitCode}). Verifique se está instalado (ver README). " +
                $"Detalhe: {stderr.Trim()}");
        }
    }

    private async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in args) startInfo.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = startInfo };
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException($"yt-dlp não respondeu em {_options.TimeoutSeconds}s.");
        }

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static string ParseVtt(string vttContent)
    {
        var sb = new StringBuilder();
        string? lastLine = null;

        foreach (var rawLine in vttContent.Replace("\r\n", "\n").Split('\n'))
        {
            var line = InlineTag.Replace(rawLine, "").Trim();

            if (line.Length == 0) continue;
            if (line.StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase)) continue;
            if (line.StartsWith("Kind:", StringComparison.OrdinalIgnoreCase)) continue;
            if (line.StartsWith("Language:", StringComparison.OrdinalIgnoreCase)) continue;
            if (TimestampLine.IsMatch(line)) continue;
            if (CueNumberLine.IsMatch(line)) continue;
            if (line == lastLine) continue;

            sb.Append(line).Append(' ');
            lastLine = line;
        }

        return sb.ToString().Trim();
    }
}

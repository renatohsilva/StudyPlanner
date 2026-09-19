namespace StudyPlanner.Application.Common.Interfaces;

public record YouTubeTranscript(string Title, string TranscriptText);

/// <summary>Extrai a legenda de um vídeo do YouTube como texto — sem chave de API, sem LLM.</summary>
public interface IYouTubeTranscriptFetcher
{
    Task<YouTubeTranscript> FetchAsync(string youTubeUrl, CancellationToken cancellationToken);
}

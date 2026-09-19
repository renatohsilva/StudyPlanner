using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.IntegrationTests.Fakes;

/// <summary>Evita depender de yt-dlp/rede nos testes — devolve uma transcrição fixa.</summary>
public class FakeYouTubeTranscriptFetcher : IYouTubeTranscriptFetcher
{
    public Task<YouTubeTranscript> FetchAsync(string youTubeUrl, CancellationToken cancellationToken)
    {
        return Task.FromResult(new YouTubeTranscript(
            Title: "Vídeo de teste",
            TranscriptText: "Este é um texto de transcrição fake para testes de integração."));
    }
}

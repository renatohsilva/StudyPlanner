namespace StudyPlanner.Infrastructure.ExternalContent;

public class YtDlpOptions
{
    /// <summary>Nome/caminho do executável yt-dlp (precisa estar instalado — ver README). Confia no PATH por padrão.</summary>
    public string ExecutablePath { get; set; } = "yt-dlp";

    /// <summary>Idiomas de legenda tentados nessa ordem de preferência.</summary>
    public string SubtitleLanguages { get; set; } = "pt,en";

    public int TimeoutSeconds { get; set; } = 60;
}

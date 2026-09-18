namespace StudyPlanner.Infrastructure.Llm;

public class AnthropicLlmOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-5";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1/messages";
    public int MaxTokens { get; set; } = 4096;
}

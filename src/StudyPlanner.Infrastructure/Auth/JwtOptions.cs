namespace StudyPlanner.Infrastructure.Auth;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "StudyPlanner";
    public string Audience { get; set; } = "StudyPlanner";
    public int ExpiryMinutes { get; set; } = 60 * 24 * 7;
}

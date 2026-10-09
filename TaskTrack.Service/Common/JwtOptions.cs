namespace TaskTrack.Service.Common;

/// <summary>
/// "Jwt" configuration section. The signing secret is never stored in source code: it comes from the
/// JWT_SECRET environment variable in production (Render) and from appsettings.Development.json locally.
/// </summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "TaskTrack.API";
    public string Audience { get; set; } = "TaskTrack.Client";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}

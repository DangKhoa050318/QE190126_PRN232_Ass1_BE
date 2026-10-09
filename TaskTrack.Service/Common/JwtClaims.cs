namespace TaskTrack.Service.Common;

/// <summary>Custom claim names in the access token payload (besides the standard sub, jti, iat, exp, iss, aud).</summary>
public static class JwtClaims
{
    public const string AccountId = "accountId";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
}

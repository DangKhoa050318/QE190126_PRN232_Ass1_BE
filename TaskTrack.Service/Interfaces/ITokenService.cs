using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    /// <summary>Signed JWT carrying the account id, email, name and role.</summary>
    AccessToken CreateAccessToken(SystemAccount account);

    /// <summary>Random opaque refresh token handed to the client.</summary>
    string CreateRefreshToken();

    /// <summary>SHA-256 hex of a refresh token — only this is stored in the database.</summary>
    string HashRefreshToken(string refreshToken);
}

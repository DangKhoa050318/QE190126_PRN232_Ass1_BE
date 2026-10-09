using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    /// <summary>Signed JWT carrying the account id, email, name and role.</summary>
    AccessToken CreateAccessToken(SystemAccount account);
}

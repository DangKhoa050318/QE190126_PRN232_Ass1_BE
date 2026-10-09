using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface IRefreshTokenRepository
{
    /// <summary>Tracked token with its account, looked up by the SHA-256 hash of the token.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash);

    /// <summary>Saves the new token together with any pending change, e.g. revoking the token it replaces.</summary>
    Task<RefreshToken> AddAsync(RefreshToken token);

    Task<int> UpdateAsync(RefreshToken token);

    /// <summary>Revokes every active token of the account (ends all its sessions).</summary>
    Task<int> RevokeAllAsync(int accountId, DateTime now);

    /// <summary>Deletes expired tokens and tokens revoked more than a day ago.</summary>
    Task<int> RemoveStaleAsync(int accountId, DateTime now);
}

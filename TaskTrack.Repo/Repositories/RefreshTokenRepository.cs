using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class RefreshTokenRepository(TaskManagementDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        context.RefreshTokens.Include(t => t.Account).FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task<RefreshToken> AddAsync(RefreshToken token)
    {
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();
        return token;
    }

    public Task<int> UpdateAsync(RefreshToken token)
    {
        context.RefreshTokens.Update(token);
        return context.SaveChangesAsync();
    }

    public Task<int> RevokeAllAsync(int accountId, DateTime now) =>
        context.RefreshTokens
            .Where(t => t.AccountId == accountId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));

    public Task<int> RemoveStaleAsync(int accountId, DateTime now)
    {
        var revokedBefore = now.AddDays(-1);
        return context.RefreshTokens
            .Where(t => t.AccountId == accountId && (t.ExpiresAt <= now || t.RevokedAt < revokedBefore))
            .ExecuteDeleteAsync();
    }
}

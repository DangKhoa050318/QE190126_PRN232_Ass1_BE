using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class SystemAccountRepository(TaskManagementDbContext context) : ISystemAccountRepository
{
    public Task<List<SystemAccount>> GetAllAsync() =>
        context.SystemAccounts.AsNoTracking().OrderBy(a => a.AccountId).ToListAsync();

    public Task<SystemAccount?> GetByIdAsync(int id) =>
        context.SystemAccounts.FirstOrDefaultAsync(a => a.AccountId == id);

    public Task<SystemAccount?> FindByIdAsync(int id) =>
        context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountId == id);

    public Task<SystemAccount?> GetByEmailAsync(string email) =>
        context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);

    public Task<bool> EmailExistsAsync(string email) =>
        context.SystemAccounts.AnyAsync(a => a.Email == email);

    public Task<Dictionary<int, int>> CountCreatedTasksAsync() =>
        context.Tasks
            .Where(t => t.CreatedById != null)
            .GroupBy(t => t.CreatedById!.Value)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count);

    public Task<int> CountCreatedTasksAsync(int accountId) =>
        context.Tasks.CountAsync(t => t.CreatedById == accountId);

    public async Task<SystemAccount> AddAsync(SystemAccount account)
    {
        context.SystemAccounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<SystemAccount> UpdateAsync(SystemAccount account)
    {
        context.SystemAccounts.Update(account);
        await context.SaveChangesAsync();
        return account;
    }

    /// <summary>Refresh tokens are removed and Task.UpdatedByID is cleared by the database (FK actions).</summary>
    public async Task<bool> DeleteAsync(SystemAccount account)
    {
        context.SystemAccounts.Remove(account);
        return await context.SaveChangesAsync() > 0;
    }
}

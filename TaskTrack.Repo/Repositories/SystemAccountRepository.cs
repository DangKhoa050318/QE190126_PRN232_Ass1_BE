using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public class SystemAccountRepository(TaskManagementDbContext context) : ISystemAccountRepository
{
    public Task<SystemAccount?> GetByEmailAsync(string email) =>
        context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);

    /// <summary>Read-only lookup, used on every authenticated request.</summary>
    public Task<SystemAccount?> FindByIdAsync(int id) =>
        context.SystemAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountId == id);

    public Task<bool> EmailExistsAsync(string email) =>
        context.SystemAccounts.AnyAsync(a => a.Email == email);

    public async Task<SystemAccount> AddAsync(SystemAccount account)
    {
        context.SystemAccounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }
}

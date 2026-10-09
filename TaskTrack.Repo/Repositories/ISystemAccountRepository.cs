using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface ISystemAccountRepository
{
    Task<List<SystemAccount>> GetAllAsync();

    /// <summary>Tracked entity, used for update/delete.</summary>
    Task<SystemAccount?> GetByIdAsync(int id);

    /// <summary>Read-only lookup, used on every authenticated request.</summary>
    Task<SystemAccount?> FindByIdAsync(int id);

    /// <summary>Emails are stored lower-cased, so pass a normalized email.</summary>
    Task<SystemAccount?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);

    /// <summary>Number of tasks (including soft-deleted ones) created by each account.</summary>
    Task<Dictionary<int, int>> CountCreatedTasksAsync();
    Task<int> CountCreatedTasksAsync(int accountId);

    Task<SystemAccount> AddAsync(SystemAccount account);
    Task<SystemAccount> UpdateAsync(SystemAccount account);
    Task<bool> DeleteAsync(SystemAccount account);
}

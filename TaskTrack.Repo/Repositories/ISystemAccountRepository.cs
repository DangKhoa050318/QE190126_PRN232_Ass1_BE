using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface ISystemAccountRepository
{
    /// <summary>Emails are stored lower-cased, so pass a normalized email.</summary>
    Task<SystemAccount?> GetByEmailAsync(string email);
    Task<SystemAccount?> FindByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<SystemAccount> AddAsync(SystemAccount account);
}

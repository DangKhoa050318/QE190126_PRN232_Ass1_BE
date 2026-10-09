using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

/// <summary>Account management for admins.</summary>
public interface IAccountService
{
    Task<List<AccountDetailDto>> GetAllAsync();
    Task<AccountDetailDto> GetByIdAsync(int id);
    Task<AccountDetailDto> UpdateAsync(int id, UpdateAccountRequest request);
    Task DeleteAsync(int id);
}

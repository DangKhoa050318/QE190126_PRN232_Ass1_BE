using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface IAuthService
{
    /// <summary>Creates a Staff account (the register API never creates admins).</summary>
    Task<AccountDto> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>Current state of an account, used to validate the account behind a JWT; null if it was deleted.</summary>
    Task<AccountDto?> FindAccountAsync(int accountId);
}

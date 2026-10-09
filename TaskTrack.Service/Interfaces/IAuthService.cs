using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces;

public interface IAuthService
{
    /// <summary>Creates a Staff account (the register API never creates admins).</summary>
    Task<AccountDto> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>Rotates the refresh token: the old one is revoked and a new token pair is returned.</summary>
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);

    /// <summary>Revokes the refresh token so the session cannot be extended any more.</summary>
    Task LogoutAsync(RefreshTokenRequest request);

    /// <summary>Current state of an account, used to validate the account behind a JWT; null if it was deleted.</summary>
    Task<AccountDto?> FindAccountAsync(int accountId);
}

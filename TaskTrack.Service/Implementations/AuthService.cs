using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Common;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class AuthService(
    ISystemAccountRepository accounts,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    JwtOptions jwt) : IAuthService
{
    // A revoked refresh token presented again within this window is most likely two browser tabs refreshing
    // at the same moment; after it, the reuse is treated as a stolen token and every session is revoked.
    private static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromMinutes(1);

    // Checked when the email is unknown, so a failed login takes about the same time whether or not
    // the account exists (no account enumeration through response timing).
    private static string? dummyHash;

    public async Task<AccountDto> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (await accounts.EmailExistsAsync(email))
            throw EmailTaken(email);

        var account = new SystemAccount
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = AccountRoles.Staff,
            CreatedDate = Clock.Now,
        };

        try
        {
            await accounts.AddAsync(account);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw EmailTaken(email); // registered concurrently by another request
        }

        return account.ToDto();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var account = await accounts.GetByEmailAsync(NormalizeEmail(request.Email));
        if (account is null)
        {
            passwordHasher.Verify(request.Password, dummyHash ??= passwordHasher.Hash(Guid.NewGuid().ToString()));
            throw InvalidCredentials();
        }

        if (!passwordHasher.Verify(request.Password, account.PasswordHash))
            throw InvalidCredentials();

        await refreshTokens.RemoveStaleAsync(account.AccountId, Clock.Now);
        return await IssueTokensAsync(account);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var now = Clock.Now;
        var stored = await refreshTokens.GetByHashAsync(tokenService.HashRefreshToken(request.RefreshToken))
                     ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.RevokedAt is { } revokedAt)
        {
            if (now - revokedAt > ReuseGracePeriod)
                await refreshTokens.RevokeAllAsync(stored.AccountId, now);
            throw new UnauthorizedException("Refresh token has already been used or revoked.");
        }

        if (stored.ExpiresAt <= now)
            throw new UnauthorizedException("Refresh token has expired. Log in again.");

        // Rotation: each refresh token works once. The revocation is saved together with the new token.
        stored.RevokedAt = now;
        return await IssueTokensAsync(stored.Account);
    }

    public async Task LogoutAsync(RefreshTokenRequest request)
    {
        var stored = await refreshTokens.GetByHashAsync(tokenService.HashRefreshToken(request.RefreshToken));
        if (stored is { RevokedAt: null })
        {
            stored.RevokedAt = Clock.Now;
            await refreshTokens.UpdateAsync(stored);
        }
    }

    public async Task<AccountDto?> FindAccountAsync(int accountId) =>
        (await accounts.FindByIdAsync(accountId))?.ToDto();

    /// <summary>New access token (with the account's current name and role) plus a new refresh token.</summary>
    private async Task<AuthResponse> IssueTokensAsync(SystemAccount account)
    {
        var access = tokenService.CreateAccessToken(account);
        var refresh = tokenService.CreateRefreshToken();
        var now = Clock.Now;

        await refreshTokens.AddAsync(new RefreshToken
        {
            AccountId = account.AccountId,
            TokenHash = tokenService.HashRefreshToken(refresh),
            CreatedDate = now,
            ExpiresAt = now.AddDays(jwt.RefreshTokenDays),
        });

        return new AuthResponse
        {
            Token = access.Token,
            ExpiresAt = access.ExpiresAt,
            RefreshToken = refresh,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(jwt.RefreshTokenDays),
            Account = account.ToDto(),
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static ConflictException EmailTaken(string email) =>
        new("email", $"Email \"{email}\" is already registered.");

    // Same message for an unknown email and a wrong password.
    private static UnauthorizedException InvalidCredentials() => new("Invalid email or password.");
}

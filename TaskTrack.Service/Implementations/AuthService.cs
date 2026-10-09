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
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthService
{
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

        var token = tokenService.CreateAccessToken(account);
        return new AuthResponse { Token = token.Token, ExpiresAt = token.ExpiresAt, Account = account.ToDto() };
    }

    public async Task<AccountDto?> FindAccountAsync(int accountId) =>
        (await accounts.FindByIdAsync(accountId))?.ToDto();

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static ConflictException EmailTaken(string email) =>
        new("email", $"Email \"{email}\" is already registered.");

    // Same message for an unknown email and a wrong password.
    private static UnauthorizedException InvalidCredentials() => new("Invalid email or password.");
}

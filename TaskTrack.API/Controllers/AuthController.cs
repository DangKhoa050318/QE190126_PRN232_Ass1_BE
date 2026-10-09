using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    /// <summary>Register a new Staff account. Returns 409 if the email is already registered.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountDto>> Register(RegisterRequest request)
    {
        var account = await service.RegisterAsync(request);
        return Created($"/api/accounts/{account.AccountId}", account);
    }

    /// <summary>Log in with email and password. Returns a signed JWT, or 401 if the credentials are invalid.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
        Ok(await service.LoginAsync(request));

    /// <summary>Exchange a refresh token for a new token pair. Each refresh token works once (rotation).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request) =>
        Ok(await service.RefreshAsync(request));

    /// <summary>Log out: revoke the refresh token. Always returns 204.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await service.LogoutAsync(request);
        return NoContent();
    }
}

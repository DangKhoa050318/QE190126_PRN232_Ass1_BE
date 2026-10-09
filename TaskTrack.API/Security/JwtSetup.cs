using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskTrack.API.Middlewares;
using TaskTrack.Service.Common;
using TaskTrack.Service.Implementations;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Security;

public static class JwtSetup
{
    private const string AuthErrorKey = "TaskTrack.AuthError";

    /// <summary>
    /// Reads the "Jwt" section. The secret comes from the JWT_SECRET environment variable (Render) and falls back
    /// to Jwt:Secret (appsettings.Development.json); startup fails if neither provides a 256-bit key.
    /// </summary>
    public static JwtOptions GetJwtOptions(this IConfiguration configuration)
    {
        var options = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var secret = configuration["JWT_SECRET"];
        if (!string.IsNullOrWhiteSpace(secret))
            options.Secret = secret;

        if (Encoding.UTF8.GetByteCount(options.Secret) < 32)
            throw new InvalidOperationException(
                "JWT secret is missing or shorter than 32 characters. Set the JWT_SECRET environment variable.");
        return options;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtOptions jwt)
    {
        services.AddSingleton(jwt);
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // keep "role", "email", "name" exactly as issued
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtTokenService.SigningKey(jwt.Secret),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtClaims.Name,
                    RoleClaimType = JwtClaims.Role,
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = SyncWithAccountAsync,
                    OnChallenge = WriteUnauthorizedAsync,
                    OnForbidden = WriteForbiddenAsync,
                };
            });
        services.AddAuthorization();
        return services;
    }

    // The account behind a valid token is re-read on each authenticated request: a deleted account is rejected,
    // and a role change made by an admin takes effect immediately instead of when the token expires.
    private static async Task SyncWithAccountAsync(TokenValidatedContext context)
    {
        var claim = context.Principal?.FindFirst(JwtClaims.AccountId)?.Value;
        var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
        var account = int.TryParse(claim, out var accountId) ? await auth.FindAccountAsync(accountId) : null;
        if (account is null)
        {
            context.HttpContext.Items[AuthErrorKey] = "The account for this token no longer exists.";
            context.Fail("Account not found.");
            return;
        }

        var identity = (ClaimsIdentity)context.Principal!.Identity!;
        foreach (var role in identity.FindAll(JwtClaims.Role).ToList())
            identity.RemoveClaim(role);
        identity.AddClaim(new Claim(JwtClaims.Role, account.RoleName));
    }

    // Replace the default empty 401 with a ProblemDetails body that says why.
    private static Task WriteUnauthorizedAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.Headers.WWWAuthenticate = "Bearer";
        var detail = context.HttpContext.Items[AuthErrorKey] as string ?? context.AuthenticateFailure switch
        {
            SecurityTokenExpiredException => "The access token has expired. Log in again.",
            not null => "The access token is invalid.",
            null => "Authentication is required. Log in and send the token as 'Authorization: Bearer <token>'.",
        };
        return ProblemWriter.WriteAsync(context.HttpContext, new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = detail,
        });
    }

    private static Task WriteForbiddenAsync(ForbiddenContext context) =>
        ProblemWriter.WriteAsync(context.HttpContext, new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "You do not have permission to perform this action. It requires the Admin role.",
        });
}

using System.Security.Claims;
using TaskTrack.Service.Common;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Security;

/// <summary>Reads the current account from the claims of the validated JWT.</summary>
public class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public int? AccountId =>
        int.TryParse(User?.FindFirst(JwtClaims.AccountId)?.Value, out var id) ? id : null;

    public bool IsAdmin => User?.IsInRole(AccountRoles.AdminName) ?? false;
}

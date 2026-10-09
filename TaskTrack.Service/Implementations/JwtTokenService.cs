using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskTrack.Repo.Models;
using TaskTrack.Service.Common;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class JwtTokenService(JwtOptions options) : ITokenService
{
    private readonly JsonWebTokenHandler handler = new();

    public AccessToken CreateAccessToken(SystemAccount account)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(options.AccessTokenMinutes);

        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires, // written as the "exp" claim
            SigningCredentials = new SigningCredentials(SigningKey(options.Secret), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = account.AccountId.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
                [JwtClaims.AccountId] = account.AccountId,
                [JwtClaims.Email] = account.Email,
                [JwtClaims.Name] = account.FullName,
                [JwtClaims.Role] = AccountRoles.Name(account.Role),
            },
        });

        return new AccessToken(token, expires);
    }

    public static SymmetricSecurityKey SigningKey(string secret) => new(Encoding.UTF8.GetBytes(secret));
}

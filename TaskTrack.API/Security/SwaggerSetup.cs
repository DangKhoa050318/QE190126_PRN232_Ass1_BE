using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskTrack.API.Security;

public static class SwaggerSetup
{
    private const string SchemeId = "Bearer";

    /// <summary>Swagger with an "Authorize" button for JWTs; only protected endpoints show the lock icon.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services) =>
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TaskTrack API",
                Version = "v1",
                Description = "GET endpoints are public. Create/update/delete endpoints require a JWT: call " +
                              "POST /api/auth/login, click Authorize and paste the token. /api/accounts requires the Admin role.",
            });
            c.AddSecurityDefinition(SchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste the token returned by POST /api/auth/login (without the \"Bearer \" prefix).",
            });
            c.OperationFilter<AuthorizeOperationFilter>();
        });

    private class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var authorize = metadata.OfType<IAuthorizeData>().ToList();
            if (authorize.Count == 0 || metadata.OfType<IAllowAnonymous>().Any())
                return;

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SchemeId } }] = [],
            });
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, invalid or expired token" });
            if (authorize.Any(a => !string.IsNullOrEmpty(a.Roles)))
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Logged in, but the Admin role is required" });
        }
    }
}

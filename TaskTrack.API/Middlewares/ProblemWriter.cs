using Microsoft.AspNetCore.Mvc;

namespace TaskTrack.API.Middlewares;

/// <summary>Writes an RFC 7807 problem response (used by the exception middleware and the JWT events).</summary>
public static class ProblemWriter
{
    public static Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }
}

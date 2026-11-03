using AgentBrowser.Shared.Models;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Adds the API version header to every response.
/// </summary>
public sealed class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public ApiVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Adds X-Api-Version.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers[ApiContract.ApiVersionHeader] = ApiContract.ApiVersion;
        await _next(context);
    }
}

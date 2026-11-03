using AgentBrowser.Shared.Models;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Correlates every request with an ID for logs and error responses.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Adds a correlation ID to the context and response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[ApiContract.CorrelationIdHeader].FirstOrDefault();
        var correlationId = !string.IsNullOrWhiteSpace(supplied)
            ? supplied.Trim()
            : Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16))
                .ToLowerInvariant();

        context.Items[ApiContract.CorrelationIdHeader] = correlationId;
        context.Response.Headers[ApiContract.CorrelationIdHeader] = correlationId;
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [ApiContract.CorrelationIdHeader] = correlationId
        }))
        {
            await _next(context);
        }
    }
}

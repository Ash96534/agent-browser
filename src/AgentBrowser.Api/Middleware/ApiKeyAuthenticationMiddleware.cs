using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Responses;
using System.Security.Cryptography;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Optional API-key authentication. Configure AGENTBROWSER_API_KEY to enable it.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _requiredKey;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
        var key = Environment.GetEnvironmentVariable("AGENTBROWSER_API_KEY");
        _requiredKey = string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    /// <summary>
    /// Validates the key when one is configured.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (_requiredKey is null || IsAuthorized(context))
        {
            await _next(context);
            return;
        }

        var traceId = context.Items[ApiContract.CorrelationIdHeader] as string;
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Kind = ErrorKind.Unauthorized.ToString(),
            Error = "Authentication required.",
            Message = "Provide AGENTBROWSER_API_KEY through the X-Api-Key header or an Authorization: Bearer token.",
            TraceId = traceId,
            Retryable = false
        });
    }

    private bool IsAuthorized(HttpContext context)
    {
        var key = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(key))
        {
            var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                key = authorization["Bearer ".Length..].Trim();
            }
        }

        return FixedTimeEquals(key, _requiredKey);
    }

    private static bool FixedTimeEquals(string? supplied, string? required)
    {
        if (supplied is null || required is null)
        {
            return false;
        }

        var suppliedBytes = System.Text.Encoding.UTF8.GetBytes(supplied);
        var requiredBytes = System.Text.Encoding.UTF8.GetBytes(required);
        if (suppliedBytes.Length != requiredBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(suppliedBytes, requiredBytes);
    }
}

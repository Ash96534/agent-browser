using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Responses;
using System.Text.Json;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Rejects requests that declare a body larger than the configured limit.
/// </summary>
public sealed class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxBodyBytes;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public RequestSizeLimitMiddleware(RequestDelegate next)
    {
        _next = next;
        var configured = Environment.GetEnvironmentVariable("AGENTBROWSER_MAX_REQUEST_BYTES");
        _maxBodyBytes = long.TryParse(configured, out var parsed) && parsed > 0
            ? parsed
            : 1_000_000;
    }

    /// <summary>
    /// Invokes the pipeline or returns 413.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var contentLength = context.Request.ContentLength;
        if (contentLength is > 0 && contentLength > _maxBodyBytes)
        {
            var traceId = context.Items[ApiContract.CorrelationIdHeader] as string;
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Kind = ErrorKind.RequestTooLarge.ToString(),
                Error = "Request body is too large.",
                Message = $"Content-Length exceeds AGENTBROWSER_MAX_REQUEST_BYTES ({_maxBodyBytes} bytes).",
                TraceId = traceId,
                Retryable = false
            });
            return;
        }

        await _next(context);
    }
}

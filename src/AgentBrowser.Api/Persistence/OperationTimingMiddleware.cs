using AgentBrowser.Shared.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AgentBrowser.Api.Persistence;

/// <summary>
/// Records operation timings into the optional persistence store.
/// </summary>
public sealed class OperationTimingMiddleware
{
    private static readonly Regex SessionPathPattern = new(
        @"^/session/(?<id>[^/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly RequestDelegate _next;
    private readonly OperationStore _store;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public OperationTimingMiddleware(RequestDelegate next, OperationStore store)
    {
        _next = next;
        _store = store;
    }

    /// <summary>
    /// Records an operation unless persistence is disabled.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!_store.Enabled ||
            (path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/capabilities", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/diagnostics", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/ws", StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var timer = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            timer.Stop();
            var match = SessionPathPattern.Match(path);
            var sessionId = match.Success ? match.Groups["id"].Value : null;

            await _store.RecordAsync(new OperationRecord(
                DateTimeOffset.UtcNow,
                context.Request.Method,
                path,
                sessionId,
                context.Response.StatusCode,
                timer.ElapsedMilliseconds,
                context.Items[ApiContract.CorrelationIdHeader] as string));
        }
    }
}

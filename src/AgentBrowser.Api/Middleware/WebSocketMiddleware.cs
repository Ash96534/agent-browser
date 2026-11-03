using AgentBrowser.Api.WebSockets;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Accepts WebSocket connections on <c>/ws/session/{sessionId}</c>.
/// </summary>
public sealed class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSocketHandler _webSocketHandler;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
    {
        _next = next;
        _webSocketHandler = webSocketHandler;
    }

    /// <summary>
    /// Routes matching WebSocket requests to the handler and passes others through.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest &&
            context.Request.Path.StartsWithSegments("/ws/session", out var remaining) &&
            remaining.HasValue)
        {
            var sessionId = remaining.Value!.Trim('/');
            if (!string.IsNullOrEmpty(sessionId))
            {
                await _webSocketHandler.AcceptWebSocketAsync(context, sessionId);
                return;
            }
        }

        await _next(context);
    }
}

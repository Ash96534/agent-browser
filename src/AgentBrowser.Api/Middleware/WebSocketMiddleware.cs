using AgentBrowser.Api.WebSockets;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Middleware for handling WebSocket connections
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSocketHandler _webSocketHandler;

    public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
    {
        _next = next;
        _webSocketHandler = webSocketHandler;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_webSocketHandler.IsWebSocketRequest(context))
        {
            // Extract session ID from path
            var path = context.Request.Path.Value ?? "";
            var sessionId = path.Replace("/ws/session/", "").TrimEnd('/');

            if (!string.IsNullOrEmpty(sessionId))
            {
                await _webSocketHandler.AcceptWebSocketAsync(context, sessionId);
                return;
            }
        }

        await _next(context);
    }
}

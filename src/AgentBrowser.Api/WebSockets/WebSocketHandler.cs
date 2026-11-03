using AgentBrowser.Browser.Browser;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AgentBrowser.Api.WebSockets;

/// <summary>
/// Handles WebSocket connections and forwards browser events to the matching session.
/// </summary>
public sealed class WebSocketHandler : IBrowserEventSink
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<WebSocketHandler> _logger;

    /// <summary>
    /// Creates a WebSocket handler.
    /// </summary>
    public WebSocketHandler(ILogger<WebSocketHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Accepts a WebSocket connection and keeps it open until the client or server closes it.
    /// </summary>
    public async Task AcceptWebSocketAsync(HttpContext context, string sessionId)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        _connections[sessionId] = webSocket;
        _logger.LogInformation("WebSocket connected for session {SessionId}", sessionId);

        try
        {
            await ReceiveLoopAsync(webSocket, context.RequestAborted).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Request aborted; the finally block removes the connection.
        }
        catch (WebSocketException ex)
        {
            _logger.LogDebug(ex, "WebSocket closed with an error for session {SessionId}", sessionId);
        }
        finally
        {
            _connections.TryRemove(sessionId, out _);
            _logger.LogInformation("WebSocket disconnected for session {SessionId}", sessionId);
        }
    }

    /// <inheritdoc />
    public async ValueTask PublishAsync(BrowserEvent browserEvent, CancellationToken cancellationToken = default)
    {
        var message = WebSocketPayload.FromBrowserEvent(browserEvent);
        await SendAsync(browserEvent.SessionId, message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a message to a specific session, if connected.
    /// </summary>
    public async Task SendAsync(string sessionId, WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(sessionId, out var webSocket) ||
            webSocket.State != WebSocketState.Open)
        {
            return;
        }

        var json = JsonSerializer.Serialize(message, message.GetType(), JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of active WebSocket connections.
    /// </summary>
    public int GetConnectionCount() => _connections.Count;

    private static async Task ReceiveLoopAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                    cancellationToken).ConfigureAwait(false);
                return;
            }
        }
    }
}

using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace AgentBrowser.Api.WebSockets;

/// <summary>
/// Handles WebSocket connections and event streaming
/// </summary>
public class WebSocketHandler
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections;
    private readonly ConcurrentDictionary<string, Session> _sessions;
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<WebSocketHandler> _logger;
    private readonly object _lock = new();

    public WebSocketHandler(IBrowserManager browserManager, ILogger<WebSocketHandler> logger)
    {
        _connections = new ConcurrentDictionary<string, WebSocket>();
        _sessions = new ConcurrentDictionary<string, Session>();
        _browserManager = browserManager;
        _logger = logger;
    }

    /// <summary>
    /// Check if WebSocket request
    /// </summary>
    public bool IsWebSocketRequest(HttpContext context)
    {
        return context.WebSockets.IsWebSocketRequest;
    }

    /// <summary>
    /// Accept WebSocket connection
    /// </summary>
    public async Task AcceptWebSocketAsync(HttpContext context, string sessionId)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        _connections[sessionId] = webSocket;
        
        _logger.LogInformation("WebSocket connected for session {SessionId}", sessionId);
    }

    /// <summary>
    /// Close WebSocket connection
    /// </summary>
    public async Task CloseWebSocketAsync(string sessionId)
    {
        if (_connections.TryRemove(sessionId, out var webSocket))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _logger.LogInformation("WebSocket closed for session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// Send message to all connected clients
    /// </summary>
    public async Task BroadcastAsync(WebSocketMessage message)
    {
        foreach (var kvp in _connections.ToList())
        {
            try
            {
                await SendAsync(kvp.Key, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to session {SessionId}", kvp.Key);
            }
        }
    }

    /// <summary>
    /// Send message to specific session
    /// </summary>
    public async Task SendAsync(string sessionId, WebSocketMessage message)
    {
        if (_connections.TryGetValue(sessionId, out var webSocket) && 
            webSocket.State == WebSocketState.Open)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);

            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    /// <summary>
    /// Get active connections count
    /// </summary>
    public int GetConnectionCount()
    {
        return _connections.Count;
    }

    /// <summary>
    /// Get session IDs with active connections
    /// </summary>
    public IEnumerable<string> GetActiveSessionIds()
    {
        return _connections.Keys;
    }
}

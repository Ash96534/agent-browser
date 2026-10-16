using AgentBrowser.Shared.Models;

namespace AgentBrowser.Api.WebSockets;

/// <summary>
/// Base WebSocket message
/// </summary>
public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Console log event
/// </summary>
public class ConsoleEvent : WebSocketMessage
{
    public ConsoleEvent()
    {
        Type = "console";
    }

    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // log, warn, error, info
}

/// <summary>
/// Network event
/// </summary>
public class NetworkEvent : WebSocketMessage
{
    public NetworkEvent()
    {
        Type = "network";
    }

    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int Status { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}

/// <summary>
/// DOM update event
/// </summary>
public class DomEvent : WebSocketMessage
{
    public DomEvent()
    {
        Type = "dom-update";
    }

    public string Selector { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // added, removed, modified
    public string? Content { get; set; }
}

/// <summary>
/// Load complete event
/// </summary>
public class LoadCompleteEvent : WebSocketMessage
{
    public LoadCompleteEvent()
    {
        Type = "load-complete";
    }

    public string Url { get; set; } = string.Empty;
    public DateTime LoadTime { get; set; }
}

/// <summary>
/// Error event
/// </summary>
public class ErrorEvent : WebSocketMessage
{
    public ErrorEvent()
    {
        Type = "error";
    }

    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

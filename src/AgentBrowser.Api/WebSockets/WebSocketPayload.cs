using AgentBrowser.Browser.Browser;

namespace AgentBrowser.Api.WebSockets;

/// <summary>
/// Maps browser-layer events to WebSocket transport messages.
/// </summary>
public static class WebSocketPayload
{
    /// <summary>
    /// Creates the transport message for a browser event.
    /// </summary>
    public static WebSocketMessage FromBrowserEvent(BrowserEvent browserEvent) => browserEvent switch
    {
        BrowserConsoleEvent console => new ConsoleEvent
        {
            SessionId = console.SessionId,
            Timestamp = console.Timestamp.UtcDateTime,
            Level = console.Level,
            Message = console.Message
        },
        BrowserLoadCompleteEvent load => new LoadCompleteEvent
        {
            SessionId = load.SessionId,
            Timestamp = load.Timestamp.UtcDateTime,
            Url = load.Url,
            LoadTime = load.Timestamp.UtcDateTime
        },
        BrowserErrorEvent error => new ErrorEvent
        {
            SessionId = error.SessionId,
            Timestamp = error.Timestamp.UtcDateTime,
            Message = error.Message
        },
        _ => throw new ArgumentOutOfRangeException(nameof(browserEvent),
            $"Unsupported browser event type '{browserEvent.GetType().Name}'.")
    };
}

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Base type for events emitted by the browser layer.
/// </summary>
public abstract record BrowserEvent(string SessionId, DateTimeOffset Timestamp);

/// <summary>
/// A console message emitted by the page.
/// </summary>
public sealed record BrowserConsoleEvent(
    string SessionId,
    string Level,
    string Message,
    DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);

/// <summary>
/// A page load-complete event.
/// </summary>
public sealed record BrowserLoadCompleteEvent(
    string SessionId,
    string Url,
    DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);

/// <summary>
/// A page error event.
/// </summary>
public sealed record BrowserErrorEvent(
    string SessionId,
    string Message,
    DateTimeOffset Timestamp) : BrowserEvent(SessionId, Timestamp);

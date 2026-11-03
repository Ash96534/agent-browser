namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Base type for events emitted by the browser layer.
/// </summary>
public abstract record BrowserEvent(string SessionId, DateTimeOffset Timestamp);

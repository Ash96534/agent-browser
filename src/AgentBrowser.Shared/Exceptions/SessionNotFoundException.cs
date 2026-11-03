namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when a browser session ID is unknown or the session has already closed.
/// </summary>
public class SessionNotFoundException : BrowserException
{
    /// <summary>
    /// The missing session ID.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Creates the exception for a missing session ID.
    /// </summary>
    public SessionNotFoundException(string sessionId)
        : base($"Session '{sessionId}' not found")
    {
        SessionId = sessionId;
    }
}

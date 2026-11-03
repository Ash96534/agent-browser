namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when a response exceeds the configured byte limit.
/// </summary>
public class RequestTooLargeException : BrowserException
{
    /// <summary>
    /// Creates a request-too-large exception.
    /// </summary>
    public RequestTooLargeException(string message)
        : base(message)
    {
    }
}

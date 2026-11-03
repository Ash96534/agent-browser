namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Base class for browser control failures.
/// </summary>
public class BrowserException : Exception
{
    /// <summary>
    /// Creates a browser exception with a message.
    /// </summary>
    public BrowserException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a browser exception with a message and inner exception.
    /// </summary>
    public BrowserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

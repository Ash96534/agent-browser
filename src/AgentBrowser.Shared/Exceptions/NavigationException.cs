namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when navigation fails, times out, or receives an invalid URL.
/// </summary>
public class NavigationException : BrowserException
{
    /// <summary>
    /// Creates a navigation exception with a message and inner exception.
    /// </summary>
    public NavigationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

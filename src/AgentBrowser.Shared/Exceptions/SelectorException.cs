namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when a selector cannot be found or an interaction fails.
/// </summary>
public class SelectorException : BrowserException
{
    /// <summary>
    /// Creates a selector exception with a message and inner exception.
    /// </summary>
    public SelectorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when Chromium cannot be installed or launched.
/// </summary>
public class BrowserLaunchException : BrowserException
{
    /// <summary>
    /// Creates the exception with an actionable message and the underlying error.
    /// </summary>
    public BrowserLaunchException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when the browser or page has crashed or was closed externally.
/// </summary>
public class BrowserCrashedException : BrowserException
{
    /// <summary>
    /// Creates a browser-crashed exception.
    /// </summary>
    public BrowserCrashedException(string message)
        : base(message)
    {
    }
}

namespace AgentBrowser.Shared.Exceptions;

/// <summary>
/// Thrown when JavaScript evaluation fails.
/// </summary>
public class ScriptExecutionException : BrowserException
{
    /// <summary>
    /// Creates a script execution exception with a message and inner exception.
    /// </summary>
    public ScriptExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

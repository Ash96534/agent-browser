namespace AgentBrowser.Shared.Models.Responses;

/// <summary>
/// Stable machine-readable error classification.
/// </summary>
public enum ErrorKind
{
    Unknown,
    InvalidRequest,
    Configuration,
    SessionNotFound,
    NavigationFailure,
    SelectorNotFound,
    ScriptFailure,
    Timeout,
    ScreenshotFailure,
    BrowserLaunchFailure,
    BrowserCrashed,
    RequestTooLarge,
    Unauthorized
}

/// <summary>
/// Error response returned by the API and MCP tools.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error kind.
    /// </summary>
    public string Kind { get; set; } = ErrorKind.Unknown.ToString();

    /// <summary>
    /// Human-readable error summary. Kept for compatibility with existing clients.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error detail.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Correlation ID for support and diagnostics.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Whether the caller may reasonably retry the request.
    /// </summary>
    public bool Retryable { get; set; }

    /// <summary>
    /// Optional stack trace in Development only.
    /// </summary>
    public string? StackTrace { get; set; }
}

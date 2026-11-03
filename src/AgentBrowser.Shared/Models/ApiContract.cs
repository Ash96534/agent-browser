namespace AgentBrowser.Shared.Models;

/// <summary>
/// API contract constants.
/// </summary>
public static class ApiContract
{
    /// <summary>
    /// Current API version.
    /// </summary>
    public const string ApiVersion = "1.0.0";

    /// <summary>
    /// Current error/contract schema version.
    /// </summary>
    public const string SchemaVersion = "1.0";

    /// <summary>
    /// Header used for the API version.
    /// </summary>
    public const string ApiVersionHeader = "X-Api-Version";

    /// <summary>
    /// Header used for request correlation.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-Id";
}

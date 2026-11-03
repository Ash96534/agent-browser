namespace AgentBrowser.Shared.Models;

/// <summary>
/// Diagnostics returned by the API.
/// </summary>
public sealed record DiagnosticsResponse
{
    /// <summary>Service name.</summary>
    public string Service { get; init; } = "AgentBrowser";

    /// <summary>API version.</summary>
    public string ApiVersion { get; init; } = ApiContract.ApiVersion;

    /// <summary>Error/contract schema version.</summary>
    public string SchemaVersion { get; init; } = ApiContract.SchemaVersion;

    /// <summary>.NET framework description.</summary>
    public string Framework { get; init; } = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

    /// <summary>Operating system.</summary>
    public string OperatingSystem { get; init; } = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

    /// <summary>Service uptime in seconds.</summary>
    public double UptimeSeconds { get; init; }

    /// <summary>Active session count.</summary>
    public int SessionCount { get; init; }

    /// <summary>Active session IDs.</summary>
    public IReadOnlyList<string> ActiveSessionIds { get; init; } = Array.Empty<string>();

    /// <summary>Chromium executable path when available.</summary>
    public string? ChromiumExecutablePath { get; init; }

    /// <summary>Whether Chromium is available in the configured cache.</summary>
    public bool IsChromiumAvailable { get; init; }
}

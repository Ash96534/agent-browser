namespace AgentBrowser.Shared.Models;

/// <summary>
/// Per-session browser health status.
/// </summary>
public sealed record SessionHealthResponse(bool Active, string Reason);

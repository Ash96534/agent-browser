namespace AgentBrowser.Shared.Models;

/// <summary>
/// Final URL and title reported after navigation.
/// </summary>
public sealed record PageNavigationResult(string Url, string Title);

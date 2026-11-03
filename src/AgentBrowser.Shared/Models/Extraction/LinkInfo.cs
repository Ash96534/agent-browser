namespace AgentBrowser.Shared.Models.Extraction;

/// <summary>
/// A link found on the page.
/// </summary>
public sealed record LinkInfo(
    string? Href,
    string? Text,
    string? Title,
    string? Rel,
    string? Target);

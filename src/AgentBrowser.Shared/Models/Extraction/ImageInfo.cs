namespace AgentBrowser.Shared.Models.Extraction;

/// <summary>
/// An image found on the page with its implicit context.
/// </summary>
public sealed record ImageInfo(
    string? Src,
    string? Alt,
    string? Title,
    int? Width,
    int? Height,
    string? Caption,
    string? ParentContext);

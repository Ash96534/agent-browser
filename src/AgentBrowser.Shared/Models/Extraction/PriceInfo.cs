namespace AgentBrowser.Shared.Models.Extraction;

/// <summary>
/// A price found on the page.
/// </summary>
public sealed record PriceInfo(
    decimal? Value,
    string? Currency,
    string? CurrencySymbol,
    string? Context,
    string? Selector);

namespace AgentBrowser.Shared.Models;

/// <summary>
/// A visible, actionable page element.
/// </summary>
public sealed record ClickableElement(
    int Index,
    string Tag,
    string Type,
    string Role,
    string Text,
    string Selector,
    string Url);

/// <summary>
/// Fill request mapping labels to values.
/// </summary>
public sealed record FillByLabelRequest(Dictionary<string, string> Fields);

/// <summary>
/// Safe click request requiring explicit confirmation.
/// </summary>
public sealed record ClickSafeRequest(string Selector, bool Confirm);

/// <summary>
/// Fill operation result.
/// </summary>
public sealed record FillResult(
    int Filled,
    IReadOnlyList<string> Fields);

/// <summary>
/// Visual comparison result.
/// </summary>
public sealed record VisualDiffResult(
    bool Identical,
    int? PreviousBytes,
    int CurrentBytes,
    string Url,
    bool HadBaseline);

/// <summary>
/// Invoice extraction result.
/// </summary>
public sealed record InvoiceInfo(
    string? Vendor,
    string? InvoiceNumber,
    string? IssueDate,
    string? DueDate,
    string? Total,
    string? Currency,
    string? Source);

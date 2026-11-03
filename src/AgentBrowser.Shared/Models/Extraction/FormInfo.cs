namespace AgentBrowser.Shared.Models.Extraction;

/// <summary>
/// A field inside a form.
/// </summary>
public sealed record FormFieldInfo(
    string? Name,
    string? Type,
    string? Value,
    string? Placeholder,
    bool Required,
    string? Label);

/// <summary>
/// A form found on the page.
/// </summary>
public sealed record FormInfo(
    string? Action,
    string? Method,
    IReadOnlyList<FormFieldInfo> Fields);

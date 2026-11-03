namespace AgentBrowser.Shared.Models;

/// <summary>
/// Supported content output formats.
/// </summary>
public enum ContentFormat
{
    /// <summary>Plain visible text.</summary>
    Text,

    /// <summary>Raw HTML.</summary>
    Html,

    /// <summary>Simple Markdown.</summary>
    Markdown
}

/// <summary>
/// Parses the query-string content format parameter.
/// </summary>
public static class ContentFormatParser
{
    /// <summary>
    /// Parses the value, defaulting to <see cref="ContentFormat.Text"/>.
    /// </summary>
    public static ContentFormat ParseOrDefault(string? value) => value?.ToLowerInvariant() switch
    {
        "html" => ContentFormat.Html,
        "markdown" or "md" => ContentFormat.Markdown,
        _ => ContentFormat.Text
    };
}

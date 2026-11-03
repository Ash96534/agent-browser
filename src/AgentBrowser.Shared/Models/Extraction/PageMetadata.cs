using System.Text.Json;

namespace AgentBrowser.Shared.Models.Extraction;

/// <summary>
/// Hidden page metadata extracted from meta tags, canonical links, and JSON-LD.
/// </summary>
public sealed record PageMetadata(
    string? Title,
    string? Description,
    string? CanonicalUrl,
    IReadOnlyDictionary<string, string> OpenGraph,
    IReadOnlyDictionary<string, string> Twitter,
    IReadOnlyList<JsonElement> JsonLd,
    IReadOnlyList<MetaTag> MetaTags);

/// <summary>
/// A single meta tag.
/// </summary>
public sealed record MetaTag(string? Name, string? Property, string? Content);

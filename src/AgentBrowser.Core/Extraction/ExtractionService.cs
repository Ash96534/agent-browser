using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models.Extraction;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentBrowser.Core.Extraction;

/// <summary>
/// Service for extracting hidden data from web pages.
/// </summary>
public class ExtractionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IBrowserManager _browserManager;
    private readonly ILogger<ExtractionService> _logger;

    /// <summary>
    /// Creates an extraction service.
    /// </summary>
    public ExtractionService(IBrowserManager browserManager, ILogger<ExtractionService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    /// <summary>
    /// Extracts hidden metadata (OG tags, JSON-LD, etc.).
    /// </summary>
    public async Task<PageMetadata> ExtractMetadataAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Metadata, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<PageMetadata>(json) ??
            new PageMetadata(null, null, null,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Array.Empty<JsonElement>(),
                Array.Empty<MetaTag>());
    }

    /// <summary>
    /// Extracts prices from the page.
    /// </summary>
    public async Task<IReadOnlyList<PriceInfo>> ExtractPricesAsync(string sessionId, int? max = null, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Prices, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Limit(Deserialize<PriceInfo[]>(json) ?? Array.Empty<PriceInfo>(), max);
    }

    /// <summary>
    /// Extracts image context from the page.
    /// </summary>
    public async Task<IReadOnlyList<ImageInfo>> ExtractImagesAsync(string sessionId, int? max = null, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Images, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Limit(Deserialize<ImageInfo[]>(json) ?? Array.Empty<ImageInfo>(), max);
    }

    /// <summary>
    /// Extracts forms from the page.
    /// </summary>
    public async Task<IReadOnlyList<FormInfo>> ExtractFormsAsync(string sessionId, int? max = null, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Forms, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Limit(Deserialize<FormInfo[]>(json) ?? Array.Empty<FormInfo>(), max);
    }

    /// <summary>
    /// Extracts links from the page.
    /// </summary>
    public async Task<IReadOnlyList<LinkInfo>> ExtractLinksAsync(string sessionId, int? max = null, bool sameOrigin = false, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Links, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<LinkInfo> links = Deserialize<LinkInfo[]>(json) ?? Array.Empty<LinkInfo>();

        if (sameOrigin)
        {
            var origin = await GetOriginAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            {
                links = links.Where(link =>
                {
                    if (!Uri.TryCreate(link.Href, UriKind.Absolute, out var target))
                    {
                        return false;
                    }

                    return target.Host.Equals(originUri.Host, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }
        }

        return Limit(links, max);
    }

    private async Task<string?> GetOriginAsync(string sessionId, CancellationToken cancellationToken)
    {
        var session = await _browserManager.GetSessionAsync(sessionId).ConfigureAwait(false);
        return session?.Tabs.FirstOrDefault()?.Url;
    }

    private static IReadOnlyList<T> Limit<T>(IReadOnlyList<T> items, int? max)
    {
        if (max is null || max.Value < 0 || items.Count <= max.Value)
        {
            return items;
        }

        return items.Take(max.Value).ToList();
    }

    private static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

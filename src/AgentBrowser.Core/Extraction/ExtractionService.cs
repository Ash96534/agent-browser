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
    public async Task<IReadOnlyList<PriceInfo>> ExtractPricesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Prices, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<PriceInfo[]>(json) ?? Array.Empty<PriceInfo>();
    }

    /// <summary>
    /// Extracts image context from the page.
    /// </summary>
    public async Task<IReadOnlyList<ImageInfo>> ExtractImagesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Images, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<ImageInfo[]>(json) ?? Array.Empty<ImageInfo>();
    }

    /// <summary>
    /// Extracts forms from the page.
    /// </summary>
    public async Task<IReadOnlyList<FormInfo>> ExtractFormsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Forms, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<FormInfo[]>(json) ?? Array.Empty<FormInfo>();
    }

    /// <summary>
    /// Extracts links from the page.
    /// </summary>
    public async Task<IReadOnlyList<LinkInfo>> ExtractLinksAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var json = await _browserManager
            .EvaluateJsonFunctionAsync(sessionId, ExtractionScripts.Links, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return Deserialize<LinkInfo[]>(json) ?? Array.Empty<LinkInfo>();
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

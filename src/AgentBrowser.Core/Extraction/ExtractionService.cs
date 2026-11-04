using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
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
    /// Extracts a basic invoice schema from embedded structured data.
    /// </summary>
    public async Task<InvoiceInfo> ExtractInvoiceAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var metadata = await ExtractMetadataAsync(sessionId, cancellationToken).ConfigureAwait(false);
        foreach (var item in metadata.JsonLd)
        {
            if (!IsInvoiceLike(item))
            {
                continue;
            }

            return new InvoiceInfo(
                FindString(item, "seller", "provider", "name", "legalName"),
                FindString(item, "identifier", "invoiceNumber", "invoiceId", "orderNumber"),
                FindString(item, "datePublished", "orderDate", "invoiceDate", "created"),
                FindString(item, "paymentDueDate", "dueDate", "validThrough"),
                FindString(item, "totalPrice", "grandTotal", "total", "amount"),
                FindString(item, "priceCurrency", "currency", "invoiceCurrency"),
                "jsonld");
        }

        return new InvoiceInfo(null, null, null, null, null, null, "none");
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

    private static bool IsInvoiceLike(JsonElement element)
    {
        var type = FindString(element, "@type", "type");
        return type is not null &&
            (type.Contains("Invoice", StringComparison.OrdinalIgnoreCase) ||
             type.Contains("Order", StringComparison.OrdinalIgnoreCase) ||
             type.Contains("InvoiceOrder", StringComparison.OrdinalIgnoreCase));
    }

    private static string? FindString(JsonElement root, params string[] names)
    {
        if (root.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
        {
            return null;
        }

        foreach (var name in names)
        {
            var found = FindProperty(root, name);
            if (found is { } element)
            {
                return JsonElementToString(element);
            }
        }

        return null;
    }

    private static JsonElement? FindProperty(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }

                var nested = FindProperty(property.Value, name);
                if (nested is { } match)
                {
                    return match;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindProperty(item, name);
                if (nested is { } match)
                {
                    return match;
                }
            }
        }

        return null;
    }

    private static string? JsonElementToString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
        JsonValueKind.Object or JsonValueKind.Array => element.GetRawText(),
        _ => null
    };

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

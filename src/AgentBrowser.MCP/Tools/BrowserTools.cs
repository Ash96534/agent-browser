using AgentBrowser.Core.Extraction;
using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using AgentBrowser.Shared.Models.Extraction;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace AgentBrowser.MCP.Tools;

/// <summary>
/// MCP tools for controlling the single-tab AgentBrowser MVP.
/// </summary>
[McpServerToolType]
public static class BrowserTools
{
    /// <summary>
    /// Creates a fresh browser session.
    /// </summary>
    [McpServerTool(Name = "browse_create_session")]
    [Description("Create a new browser session. Returns the session ID to use with other tools.")]
    public static async Task<SessionResponse> CreateSession(
        IServiceProvider services,
        bool headless = true,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var sessionService = services.GetRequiredService<ISessionService>();
        return await sessionService.CreateSessionAsync(new SessionOptions { Headless = headless })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Closes a browser session.
    /// </summary>
    [McpServerTool(Name = "browse_close_session")]
    [Description("Close a browser session and release its Chromium process.")]
    public static async Task<bool> CloseSession(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var sessionService = services.GetRequiredService<ISessionService>();
        return await sessionService.CloseSessionAsync(sessionId).ConfigureAwait(false);
    }

    /// <summary>
    /// Navigates a session to a URL.
    /// </summary>
    [McpServerTool(Name = "browse_navigate")]
    [Description("Navigate a session to an absolute URL.")]
    public static async Task<NavigateResponse> Navigate(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Absolute URL, including the scheme.")] string url,
        [Description("One of Load, DOMContentLoaded, NetworkIdle0, or NetworkIdle2.")]
        string? waitUntil = null,
        [Description("Navigation timeout in milliseconds. Defaults to 30000.")]
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var navigationService = services.GetRequiredService<INavigationService>();
        var request = new NavigateRequest
        {
            Url = url,
            Timeout = timeout,
            WaitUntil = ParseWaitUntil(waitUntil)
        };

        return await navigationService.NavigateAsync(sessionId, request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Clicks an element by CSS selector.
    /// </summary>
    [McpServerTool(Name = "browse_click")]
    [Description("Click an element by CSS selector.")]
    public static async Task<bool> Click(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("CSS selector for the element to click.")] string selector,
        [Description("Delay between mouse down and up in milliseconds.")] int delay = 0,
        [Description("0 = left, 1 = middle, 2 = right.")] int button = 0,
        [Description("Number of clicks.")] int clickCount = 1,
        CancellationToken cancellationToken = default)
    {
        var domService = services.GetRequiredService<IDomService>();
        var request = new ClickRequest
        {
            Selector = selector,
            Options = new ClickOptions
            {
                Delay = delay,
                Button = button,
                ClickCount = clickCount
            }
        };

        await domService.ClickAsync(sessionId, request, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Types text into an element by CSS selector.
    /// </summary>
    [McpServerTool(Name = "browse_type")]
    [Description("Type text into an element by CSS selector.")]
    public static async Task<bool> Type(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("CSS selector for the input element.")] string selector,
        [Description("Text to type.")] string text,
        [Description("Delay between keystrokes in milliseconds.")] int delay = 0,
        [Description("Clear the field before typing.")] bool clearFirst = true,
        CancellationToken cancellationToken = default)
    {
        var domService = services.GetRequiredService<IDomService>();
        var request = new TypeRequest
        {
            Selector = selector,
            Text = text,
            Delay = delay,
            ClearFirst = clearFirst
        };

        await domService.TypeAsync(sessionId, request, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the rendered HTML for a session.
    /// </summary>
    [McpServerTool(Name = "browse_get_html")]
    [Description("Get the rendered HTML for a session.")]
    public static async Task<string> GetHtml(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        var navigationService = services.GetRequiredService<INavigationService>();
        var response = await navigationService.GetHtmlAsync(sessionId, cancellationToken)
            .ConfigureAwait(false);
        return response.Html;
    }

    /// <summary>
    /// Captures a screenshot of the page.
    /// </summary>
    [McpServerTool(Name = "browse_get_screenshot")]
    [Description("Capture a screenshot of the page as base64 image data.")]
    public static async Task<ScreenshotResponse> GetScreenshot(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Capture the full scrollable page instead of the viewport.")] bool fullPage = false,
        [Description("Image format: png, jpeg, jpg, or webp.")] string format = "png",
        [Description("JPEG/WebP quality from 0 to 100.")] int? quality = null,
        CancellationToken cancellationToken = default)
    {
        var screenshotService = services.GetRequiredService<IScreenshotService>();
        var request = new ScreenshotRequest
        {
            Type = fullPage ? AgentBrowser.Shared.Models.ScreenshotType.FullPage : AgentBrowser.Shared.Models.ScreenshotType.Viewport,
            FullPage = fullPage,
            Format = format,
            Quality = quality
        };

        return await screenshotService.CaptureAsync(sessionId, request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes JavaScript and returns its JSON value.
    /// </summary>
    [McpServerTool(Name = "browse_execute_js")]
    [Description("Execute JavaScript in the page and return the JSON value.")]
    public static async Task<JsonElement> ExecuteJs(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("JavaScript expression or function source to evaluate.")] string script,
        CancellationToken cancellationToken = default)
    {
        var domService = services.GetRequiredService<IDomService>();
        var request = new ExecuteJsRequest { Script = script };
        return await domService.ExecuteJsAsync<JsonElement>(sessionId, request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts hidden page metadata.
    /// </summary>
    [McpServerTool(Name = "browse_get_metadata")]
    [Description("Extract hidden metadata such as Open Graph, Twitter cards, canonical URLs, and JSON-LD.")]
    public static async Task<PageMetadata> GetMetadata(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        var extractionService = services.GetRequiredService<ExtractionService>();
        return await extractionService.ExtractMetadataAsync(sessionId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts prices from the page.
    /// </summary>
    [McpServerTool(Name = "browse_get_prices")]
    [Description("Extract prices from data attributes, itemprop markup, and currency patterns.")]
    public static async Task<IReadOnlyList<PriceInfo>> GetPrices(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Maximum number of prices to return.")] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var extractionService = services.GetRequiredService<ExtractionService>();
        return await extractionService.ExtractPricesAsync(sessionId, max, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts images with context from the page.
    /// </summary>
    [McpServerTool(Name = "browse_get_images")]
    [Description("Extract images with alt text, dimensions, captions, and parent context.")]
    public static async Task<IReadOnlyList<ImageInfo>> GetImages(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Maximum number of images to return.")] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var extractionService = services.GetRequiredService<ExtractionService>();
        return await extractionService.ExtractImagesAsync(sessionId, max, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts forms from the page.
    /// </summary>
    [McpServerTool(Name = "browse_get_forms")]
    [Description("Extract forms, fields, labels, and required attributes.")]
    public static async Task<IReadOnlyList<FormInfo>> GetForms(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Maximum number of forms to return.")] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var extractionService = services.GetRequiredService<ExtractionService>();
        return await extractionService.ExtractFormsAsync(sessionId, max, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts links from the page.
    /// </summary>
    [McpServerTool(Name = "browse_get_links")]
    [Description("Extract links with href, text, title, rel, and target.")]
    public static async Task<IReadOnlyList<LinkInfo>> GetLinks(
        IServiceProvider services,
        [Description("The session ID returned by browse_create_session.")] string sessionId,
        [Description("Maximum number of links to return.")] int? max = null,
        [Description("Only return links on the same origin as the current page.")] bool sameOrigin = false,
        CancellationToken cancellationToken = default)
    {
        var extractionService = services.GetRequiredService<ExtractionService>();
        return await extractionService.ExtractLinksAsync(sessionId, max, sameOrigin, cancellationToken)
            .ConfigureAwait(false);
    }

    private static WaitUntil? ParseWaitUntil(string? waitUntil)
    {
        if (string.IsNullOrWhiteSpace(waitUntil))
        {
            return null;
        }

        if (Enum.TryParse<WaitUntil>(waitUntil, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Unknown waitUntil value '{waitUntil}'. Use Load, DOMContentLoaded, NetworkIdle0, or NetworkIdle2.",
            nameof(waitUntil));
    }
}

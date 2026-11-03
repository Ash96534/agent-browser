using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

/// <summary>
/// Maps browser navigation results to API response models.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<NavigationService> _logger;

    /// <summary>
    /// Creates a navigation service.
    /// </summary>
    public NavigationService(IBrowserManager browserManager, ILogger<NavigationService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<NavigateResponse> NavigateAsync(
        string sessionId,
        NavigateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _browserManager
            .NavigateAsync(sessionId, request, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Navigated session {SessionId} to {Url}", sessionId, result.Url);

        return new NavigateResponse
        {
            Success = true,
            Url = result.Url,
            Title = result.Title,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<HtmlResponse> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var html = await _browserManager.GetHtmlAsync(sessionId, cancellationToken).ConfigureAwait(false);
        return new HtmlResponse { Html = html, Format = "html" };
    }

    private static string FormatName(ContentFormat format) => format switch
    {
        ContentFormat.Html => "html",
        ContentFormat.Markdown => "markdown",
        _ => "text"
    };

    /// <inheritdoc />
    public async Task<HtmlResponse> GetContentAsync(string sessionId, ContentFormat format = ContentFormat.Text, CancellationToken cancellationToken = default)
    {
        var content = await _browserManager.GetContentAsync(sessionId, format, cancellationToken)
            .ConfigureAwait(false);
        return new HtmlResponse { Html = content, Format = FormatName(format) };
    }
}

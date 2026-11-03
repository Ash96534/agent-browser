using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Core.Services;

/// <summary>
/// Navigation and page-content operations.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates a session to a URL.
    /// </summary>
    Task<NavigateResponse> NavigateAsync(string sessionId, NavigateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the rendered HTML.
    /// </summary>
    Task<HtmlResponse> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the visible body text.
    /// </summary>
    Task<HtmlResponse> GetContentAsync(string sessionId, CancellationToken cancellationToken = default);
}

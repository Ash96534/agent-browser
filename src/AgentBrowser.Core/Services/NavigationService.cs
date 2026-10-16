using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

public class NavigationService : INavigationService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(IBrowserManager browserManager, ILogger<NavigationService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<NavigateResponse> NavigateAsync(string sessionId, string tabId, NavigateRequest request)
    {
        var result = await _browserManager.NavigateAsync(sessionId, tabId, request.Url, request);
        
        return new NavigateResponse
        {
            Success = true,
            Url = request.Url
        };
    }

    public async Task<HtmlResponse> GetHtmlAsync(string sessionId, string tabId)
    {
        var session = await _browserManager.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Session '{sessionId}' not found");
        }

        // In a real implementation, would get HTML from the page
        return new HtmlResponse
        {
            Html = string.Empty
        };
    }

    public async Task<HtmlResponse> GetContentAsync(string sessionId, string tabId)
    {
        var session = await _browserManager.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Session '{sessionId}' not found");
        }

        // In a real implementation, would get content from the page
        return new HtmlResponse
        {
            Html = string.Empty
        };
    }
}

using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Core.Services;

public interface INavigationService
{
    Task<NavigateResponse> NavigateAsync(string sessionId, string tabId, NavigateRequest request);
    Task<HtmlResponse> GetHtmlAsync(string sessionId, string tabId);
    Task<HtmlResponse> GetContentAsync(string sessionId, string tabId);
}
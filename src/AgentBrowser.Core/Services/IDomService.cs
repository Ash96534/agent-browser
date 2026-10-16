using AgentBrowser.Shared.Models.Requests;

namespace AgentBrowser.Core.Services;

public interface IDomService
{
    Task ClickAsync(string sessionId, string tabId, ClickRequest request);
    Task TypeAsync(string sessionId, string tabId, TypeRequest request);
    Task<T> ExecuteJsAsync<T>(string sessionId, string tabId, ExecuteJsRequest request);
    Task WaitForSelectorAsync(string sessionId, string tabId, string selector, int timeout = 30000);
}
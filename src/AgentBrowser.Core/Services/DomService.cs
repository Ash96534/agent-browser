using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Common;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

public class DomService : IDomService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<DomService> _logger;

    public DomService(IBrowserManager browserManager, ILogger<DomService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task ClickAsync(string sessionId, string tabId, ClickRequest request)
    {
        await _browserManager.ClickAsync(sessionId, tabId, request.Selector, request.Options);
        _logger.LogInformation("Clicked element with selector {Selector}", request.Selector);
    }

    public async Task TypeAsync(string sessionId, string tabId, TypeRequest request)
    {
        await _browserManager.TypeAsync(sessionId, tabId, request.Selector, request.Text, request.Delay);
        _logger.LogInformation("Typed text into element with selector {Selector}", request.Selector);
    }

    public async Task<T> ExecuteJsAsync<T>(string sessionId, string tabId, ExecuteJsRequest request)
    {
        var result = await _browserManager.EvaluateExpressionAsync(sessionId, tabId, request.Script);
        _logger.LogInformation("Executed JavaScript: {Script}", request.Script);
        
        // Cast result to expected type
        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task WaitForSelectorAsync(string sessionId, string tabId, string selector, int timeout = 30000)
    {
        // In a real implementation, would use tab.WaitForSelectorAsync
        await Task.Delay(timeout);
        _logger.LogInformation("Waited for selector {Selector}", selector);
    }
}

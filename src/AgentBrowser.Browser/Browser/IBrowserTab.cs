using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Common;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

public interface IBrowserTab : IDisposable
{
    string Id { get; }
    string? Url { get; }
    string? Title { get; }
    TabStatus Status { get; }
    
    Task NavigateAsync(string url, NavigationOptions? options = null);
    Task<string> GetHtmlAsync();
    Task<string> GetContentAsync();
    Task<byte[]> ScreenshotAsync(AgentBrowser.Shared.Models.Requests.ScreenshotOptions? options = null);
    Task ClickAsync(string selector, AgentBrowser.Shared.Models.Requests.ClickOptions? options = null);
    Task TypeAsync(string selector, string text, int delay = 0);
    Task<ElementHandle?> QuerySelectorAsync(string selector);
    Task<IElementHandle[]> QuerySelectorAllAsync(string selector);
    Task<T> EvaluateExpressionAsync<T>(string script);
    Task WaitForSelectorAsync(string selector, WaitForOptions? options = null);
    Task WaitForNavigationAsync(WaitUntil waitUntil = WaitUntil.Load);
    Task CloseAsync();
}
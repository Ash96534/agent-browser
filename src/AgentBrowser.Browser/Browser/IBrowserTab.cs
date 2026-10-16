using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Common;
using AgentBrowser.Shared.Models.Requests;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

public interface IBrowserTab : IDisposable
{
    string Id { get; }
    string? Url { get; }
    string? Title { get; }
    TabStatus Status { get; }
    
    Task NavigateAsync(string url, NavigateOptions? options = null);
    Task<string> GetHtmlAsync();
    Task<string> GetContentAsync();
    Task<byte[]> ScreenshotAsync(ScreenshotOptions? options = null);
    Task ClickAsync(string selector, ClickOptions? options = null);
    Task TypeAsync(string selector, string text, int delay = 0);
    Task<ElementHandle?> QuerySelectorAsync(string selector);
    Task<IElementHandle[]> QuerySelectorAllAsync(string selector);
    Task<T> EvaluateExpressionAsync<T>(string script);
    Task WaitForSelectorAsync(string selector, WaitForOptions? options = null);
    Task WaitForNavigationAsync(WaitUntil waitUntil = WaitUntil.Load);
    Task CloseAsync();
}
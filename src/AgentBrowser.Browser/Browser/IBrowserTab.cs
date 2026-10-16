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
    AgentBrowser.Shared.Models.TabStatus Status { get; }
    
    Task NavigateAsync(string url, AgentBrowser.Shared.Models.NavigateOptions? options = null);
    Task<string> GetHtmlAsync();
    Task<string> GetContentAsync();
    Task<byte[]> ScreenshotAsync(AgentBrowser.Shared.Models.ScreenshotOptions? options = null);
    Task ClickAsync(string selector, AgentBrowser.Shared.Models.ClickOptions? options = null);
    Task TypeAsync(string selector, string text, int delay = 0);
    Task<ElementHandle?> QuerySelectorAsync(string selector);
    Task<IElementHandle[]> QuerySelectorAllAsync(string selector);
    Task<T> EvaluateExpressionAsync<T>(string script);
    Task WaitForSelectorAsync(string selector, AgentBrowser.Shared.Models.WaitForOptions? options = null);
    Task WaitForNavigationAsync(AgentBrowser.Shared.Models.WaitUntil waitUntil = AgentBrowser.Shared.Models.WaitUntil.Load);
    Task CloseAsync();
}
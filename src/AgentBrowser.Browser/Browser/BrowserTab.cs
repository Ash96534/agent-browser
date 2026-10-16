using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

public class BrowserTab : IBrowserTab
{
    private readonly IPage _page;
    private bool _disposed;

    public string Id { get; }
    public string? Url => _page.Url;
    public string? Title => _page.Title;
    public AgentBrowser.Shared.Models.TabStatus Status { get; private set; } = AgentBrowser.Shared.Models.TabStatus.Loaded;

    public BrowserTab(string id, IPage page)
    {
        Id = id;
        _page = page;
    }

    public async Task NavigateAsync(string url, NavigateOptions? options = null)
    {
        try
        {
            var navigationOptions = new NavigationOptions
            {
                Timeout = options?.Timeout ?? 30000,
                WaitUntil = options?.WaitUntil ?? WaitUntil.Load
            };
            
            await _page.GoToAsync(url, navigationOptions);
            Status = AgentBrowser.Shared.Models.TabStatus.Loaded;
        }
        catch (Exception ex)
        {
            Status = AgentBrowser.Shared.Models.TabStatus.Error;
            throw new NavigationException($"Failed to navigate to {url}", ex);
        }
    }

    public async Task<string> GetHtmlAsync()
    {
        return await _page.GetContentAsync();
    }

    public async Task<string> GetContentAsync()
    {
        return await _page.GetContentAsync();
    }

    public async Task<byte[]> ScreenshotAsync(ScreenshotOptions? options = null)
    {
        var screenshotType = options?.Type ?? AgentBrowser.Shared.Models.ScreenshotType.Viewport;
        
        return screenshotType switch
        {
            AgentBrowser.Shared.Models.ScreenshotType.Element when !string.IsNullOrEmpty(options?.Selector) => 
                await _page.ScreenshotElementAsync(options.Selector, new ScreenshotOptions { Type = GetImageType(options) }),
            AgentBrowser.Shared.Models.ScreenshotType.FullPage => 
                await _page.ScreenshotAsync(new ScreenshotOptions { FullPage = true, Type = GetImageType(options) }),
            _ => 
                await _page.ScreenshotAsync(new ScreenshotOptions { Type = GetImageType(options) })
        };
    }

    public async Task ClickAsync(string selector, ClickOptions? options = null)
    {
        try
        {
            var element = await _page.QuerySelectorAsync(selector);
            if (element == null)
            {
                throw new SelectorException($"Element with selector '{selector}' not found");
            }
            
            var clickOptions = new ClickOptions
            {
                Delay = options?.Delay ?? 0,
                Button = options?.Button ?? 0,
                ClickCount = options?.ClickCount ?? 1
            };
            
            await element.ClickAsync(clickOptions);
        }
        catch (SelectorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SelectorException($"Failed to click element '{selector}'", ex);
        }
    }

    public async Task TypeAsync(string selector, string text, int delay = 0)
    {
        try
        {
            var element = await _page.QuerySelectorAsync(selector);
            if (element == null)
            {
                throw new SelectorException($"Element with selector '{selector}' not found");
            }
            
            await element.TypeAsync(text, new TypeOptions { Delay = delay });
        }
        catch (SelectorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SelectorException($"Failed to type in element '{selector}'", ex);
        }
    }

    public async Task<ElementHandle?> QuerySelectorAsync(string selector)
    {
        return await _page.QuerySelectorAsync(selector);
    }

    public async Task<IElementHandle[]> QuerySelectorAllAsync(string selector)
    {
        return await _page.QuerySelectorAllAsync(selector);
    }

    public async Task<T> EvaluateExpressionAsync<T>(string script)
    {
        try
        {
            return await _page.EvaluateExpressionAsync<T>(script);
        }
        catch (Exception ex)
        {
            throw new ScriptExecutionException($"Failed to execute script: {script}", ex);
        }
    }

    public async Task WaitForSelectorAsync(string selector, WaitForOptions? options = null)
    {
        var timeout = options?.Timeout ?? 30000;
        await _page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = timeout });
    }

    public async Task WaitForNavigationAsync(WaitUntil waitUntil = WaitUntil.Load)
    {
        await _page.WaitForNavigationAsync(new WaitForNavigationOptions { WaitUntil = waitUntil });
    }

    public async Task CloseAsync()
    {
        if (_disposed) return;
        
        try
        {
            await _page.CloseAsync();
        }
        catch (Exception ex)
        {
            // Log but don't throw on close
        }
        
        _disposed = true;
    }

    public void Dispose()
    {
        CloseAsync().GetAwaiter().GetResult();
    }

    private ScreenshotType GetImageType(ScreenshotOptions? options)
    {
        return string.Equals(options?.Format, "jpeg", StringComparison.OrdinalIgnoreCase) 
            ? ScreenshotType.Jpeg 
            : ScreenshotType.Png;
    }
}
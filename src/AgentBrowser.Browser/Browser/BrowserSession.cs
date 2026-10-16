using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

public class BrowserSession : IBrowserSession
{
    private readonly ConcurrentDictionary<string, BrowserTab> _tabs = new();
    private readonly BrowserFactory _browserFactory;
    private readonly SessionOptions _options;
    
    public string Id { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastAccessedAt { get; set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Active;
    
    public IReadOnlyList<IBrowserTab> Tabs => _tabs.Values.Cast<IBrowserTab>().ToList();

    private IBrowser? _browser;
    private bool _disposed;

    public BrowserSession(string id, BrowserFactory browserFactory, SessionOptions options)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        _browserFactory = browserFactory;
        _options = options;
    }

    public async Task<IBrowserTab> CreateTabAsync()
    {
        var browser = await GetBrowserAsync();
        var page = await browser.NewPageAsync();
        
        var tabId = Guid.NewGuid().ToString();
        var tab = new BrowserTab(tabId, page);
        
        _tabs[tabId] = tab;
        
        return tab;
    }

    public async Task<IBrowserTab?> GetTabAsync(string tabId)
    {
        if (_tabs.TryGetValue(tabId, out var tab))
        {
            return tab;
        }
        
        return null;
    }

    public async Task CloseTabAsync(string tabId)
    {
        if (_tabs.TryRemove(tabId, out var tab))
        {
            await tab.CloseAsync();
        }
    }

    public async Task CloseAsync()
    {
        if (_disposed) return;
        
        Status = SessionStatus.Closed;
        
        foreach (var tab in _tabs.Values)
        {
            await tab.CloseAsync();
        }
        
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }
        
        _disposed = true;
    }

    public void Dispose()
    {
        CloseAsync().GetAwaiter().GetResult();
    }

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser == null)
        {
            _browser = await _browserFactory.CreateBrowserAsync(_options);
        }
        
        return _browser;
    }
}
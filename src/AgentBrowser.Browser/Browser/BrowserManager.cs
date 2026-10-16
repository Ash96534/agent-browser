using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Implementation of IBrowserManager using PuppeteerSharp
/// </summary>
public class BrowserManager : IBrowserManager, IDisposable
{
    private readonly BrowserFetcher _browserFetcher;
    private readonly Dictionary<string, Session> _sessions;
    private readonly object _lock = new();
    private readonly ILogger<BrowserManager> _logger;
    private bool _disposed;

    public BrowserManager(ILogger<BrowserManager> logger)
    {
        _logger = logger;
        _sessions = new Dictionary<string, Session>();
        _browserFetcher = new BrowserFetcher();
    }

    /// <summary>
    /// Ensures Chromium is available, downloads if necessary
    /// </summary>
    public async Task EnsureBrowserAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserManager));

        // Download default Chromium revision
        _logger.LogInformation("Downloading Chromium...");
        await _browserFetcher.DownloadAsync();
        _logger.LogInformation("Chromium download complete");
    }

    public async Task<Session> CreateSessionAsync(SessionOptions options)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserManager));

        var sessionId = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow;

        // Ensure browser is available
        await EnsureBrowserAsync();

        // Launch browser
        var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = options.Headless
        });

        // Create page
        var page = await browser.NewPageAsync();

        // Set viewport
        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = options.Viewport?.Width ?? 1920,
            Height = options.Viewport?.Height ?? 1080
        });

        // Set user agent
        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            await page.SetUserAgentAsync(options.UserAgent);
        }

        // Create session
        var session = new Session
        {
            Id = sessionId,
            CreatedAt = createdAt,
            LastAccessedAt = createdAt,
            Status = SessionStatus.Active,
            Viewport = options.Viewport ?? new Viewport(),
            UserAgent = options.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            Tabs = new List<Tab>
            {
                new Tab
                {
                    Id = sessionId, // For now, session ID = tab ID
                    CreatedAt = createdAt,
                    Status = TabStatus.Loading
                }
            }
        };

        lock (_lock)
        {
            _sessions[sessionId] = session;
        }

        _logger.LogInformation("Created session {SessionId}", sessionId);

        return session;
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserManager));

        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.LastAccessedAt = DateTime.UtcNow;
                return session;
            }
        }

        return null;
    }

    public async Task<bool> CloseSessionAsync(string sessionId)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserManager));

        Session? session;
        lock (_lock)
        {
            if (!_sessions.TryGetValue(sessionId, out session))
            {
                return false;
            }
        }

        try
        {
            // Close browser
            // Note: In a real implementation, we'd store the browser instance with the session
            _logger.LogInformation("Closed session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing session {SessionId}", sessionId);
            return false;
        }

        lock (_lock)
        {
            _sessions.Remove(sessionId);
        }

        return true;
    }

    public async Task<IEnumerable<Session>> GetAllSessionsAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(BrowserManager));

        lock (_lock)
        {
            return _sessions.Values.ToList();
        }
    }

    public async Task<object> NavigateAsync(string sessionId, string tabId, string url, object? options = null)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException($"Session '{sessionId}' not found");

        // Note: Actual navigation implementation would use the stored browser/page
        _logger.LogInformation("Navigating session {SessionId} to {Url}", sessionId, url);

        // Update tab URL
        var tab = session.Tabs.FirstOrDefault(t => t.Id == tabId || t.Id == sessionId);
        if (tab != null)
        {
            tab.Url = url;
            tab.Status = TabStatus.Loaded;
        }

        return new { success = true, url = url };
    }

    public async Task<string> ScreenshotAsync(string sessionId, string tabId, object options)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException($"Session '{sessionId}' not found");

        // Note: Actual screenshot implementation would use the stored browser/page
        _logger.LogInformation("Taking screenshot of session {SessionId}", sessionId);

        // Return empty base64 for now (would be actual screenshot data)
        return "";
    }

    public async Task<object> EvaluateExpressionAsync(string sessionId, string tabId, string script)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException($"Session '{sessionId}' not found");

        _logger.LogInformation("Evaluating JS in session {SessionId}", sessionId);

        // Return placeholder result
        return new { result = "evaluated", script = script };
    }

    public async Task ClickAsync(string sessionId, string tabId, string selector, object? options = null)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException($"Session '{sessionId}' not found");

        _logger.LogInformation("Clicking selector {Selector} in session {SessionId}", selector, sessionId);
    }

    public async Task TypeAsync(string sessionId, string tabId, string selector, string text, int delay = 0)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException($"Session '{sessionId}' not found");

        _logger.LogInformation("Typing text into selector {Selector} in session {SessionId}", selector, sessionId);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        // Cleanup all sessions
        foreach (var sessionId in _sessions.Keys.ToList())
        {
            CloseSessionAsync(sessionId).GetAwaiter().GetResult();
        }
    }
}

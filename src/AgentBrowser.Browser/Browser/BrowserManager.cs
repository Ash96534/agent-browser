using System.Collections.Concurrent;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// PuppeteerSharp-backed browser session manager.
/// </summary>
public sealed class BrowserManager : IBrowserManager
{
    private sealed class SessionHandle : IAsyncDisposable
    {
        public required Session Session { get; init; }
        public required IBrowser Browser { get; init; }
        public required IPage Page { get; init; }
        public SemaphoreSlim Gate { get; } = new(1, 1);

        public ValueTask DisposeAsync()
        {
            Gate.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private readonly IBrowserEngine _engine;
    private readonly IBrowserEventSink _eventSink;
    private readonly ILogger<BrowserManager> _logger;
    private readonly ConcurrentDictionary<string, SessionHandle> _sessions = new();
    private bool _disposed;

    /// <summary>
    /// Creates a browser manager.
    /// </summary>
    public BrowserManager(
        IBrowserEngine engine,
        IBrowserEventSink eventSink,
        ILogger<BrowserManager> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _eventSink = eventSink ?? throw new ArgumentNullException(nameof(eventSink));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Session> CreateSessionAsync(
        SessionOptions options,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(options);

        IBrowser? browser = null;
        try
        {
            browser = await _engine.LaunchAsync(new LaunchOptions
            {
                Headless = options.Headless,
                Args = options.Args?.ToArray()
            }, cancellationToken).ConfigureAwait(false);

            var page = await browser.NewPageAsync().ConfigureAwait(false);
            var viewport = options.Viewport ?? new Viewport();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = viewport.Width,
                Height = viewport.Height,
                DeviceScaleFactor = viewport.DeviceScaleFactor,
                IsMobile = viewport.IsMobile,
                HasTouch = viewport.HasTouch
            }).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(options.UserAgent))
            {
                await page.SetUserAgentAsync(options.UserAgent).ConfigureAwait(false);
            }

            var now = DateTime.UtcNow;
            var sessionId = Guid.NewGuid().ToString("N");
            var session = new Session
            {
                Id = sessionId,
                CreatedAt = now,
                LastAccessedAt = now,
                Status = SessionStatus.Active,
                Viewport = viewport,
                UserAgent = options.UserAgent ??
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                Tabs =
                [
                    new Tab
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        CreatedAt = now,
                        Status = TabStatus.Loading
                    }
                ]
            };

            var handle = new SessionHandle
            {
                Session = session,
                Browser = browser,
                Page = page
            };

            if (!_sessions.TryAdd(sessionId, handle))
            {
                await handle.DisposeAsync().ConfigureAwait(false);
                await browser.CloseAsync().ConfigureAwait(false);
                throw new BrowserLaunchException(
                    "Failed to register the new browser session.",
                    new InvalidOperationException("Duplicate session ID."));
            }

            _logger.LogInformation("Created session {SessionId}", sessionId);
            return session;
        }
        catch (OperationCanceledException)
        {
            await CloseQuietlyAsync(browser).ConfigureAwait(false);
            throw;
        }
        catch (Exception ex) when (ex is not BrowserException)
        {
            await CloseQuietlyAsync(browser).ConfigureAwait(false);
            throw new BrowserLaunchException(
                "Failed to launch Chromium. Verify that Chromium can be downloaded and its system dependencies are installed.",
                ex);
        }
    }

    /// <inheritdoc />
    public Task<Session?> GetSessionAsync(string sessionId)
    {
        ThrowIfDisposed();

        if (_sessions.TryGetValue(sessionId, out var handle))
        {
            handle.Session.LastAccessedAt = DateTime.UtcNow;
            return Task.FromResult<Session?>(handle.Session);
        }

        return Task.FromResult<Session?>(null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Session>> GetAllSessionsAsync()
    {
        ThrowIfDisposed();
        IReadOnlyList<Session> sessions = _sessions.Values
            .Select(handle => handle.Session)
            .ToList();
        return Task.FromResult(sessions);
    }

    /// <inheritdoc />
    public async Task<bool> CloseSessionAsync(string sessionId)
    {
        ThrowIfDisposed();

        if (!_sessions.TryRemove(sessionId, out var handle))
        {
            return false;
        }

        try
        {
            await handle.Browser.CloseAsync().ConfigureAwait(false);
            _logger.LogInformation("Closed session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing browser for session {SessionId}", sessionId);
        }
        finally
        {
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        return true;
    }

    /// <inheritdoc />
    public Task<object> NavigateAsync(string sessionId, string tabId, string url, object? options = null)
        => throw new NotSupportedException("Navigation is implemented in Task 4.");

    /// <inheritdoc />
    public Task<string> ScreenshotAsync(string sessionId, string tabId, object options)
        => throw new NotSupportedException("Screenshots are implemented in Task 6.");

    /// <inheritdoc />
    public Task<object> EvaluateExpressionAsync(string sessionId, string tabId, string script)
        => throw new NotSupportedException("JavaScript evaluation is implemented in Task 5.");

    /// <inheritdoc />
    public Task ClickAsync(string sessionId, string tabId, string selector, object? options = null)
        => throw new NotSupportedException("Click is implemented in Task 5.");

    /// <inheritdoc />
    public Task TypeAsync(string sessionId, string tabId, string selector, string text, int delay = 0)
        => throw new NotSupportedException("Type is implemented in Task 5.");

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var sessionId in _sessions.Keys.ToList())
        {
            await CloseSessionAsync(sessionId).ConfigureAwait(false);
        }

        _disposed = true;
    }

    private async Task CloseQuietlyAsync(IBrowser? browser)
    {
        if (browser is null)
        {
            return;
        }

        try
        {
            await browser.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to close a browser after a launch error");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BrowserManager));
        }
    }
}

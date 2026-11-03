using System.Collections.Concurrent;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using AgentBrowserNavigationException = AgentBrowser.Shared.Exceptions.NavigationException;
using AgentBrowserScriptExecutionException = AgentBrowser.Shared.Exceptions.ScriptExecutionException;
using AgentBrowserSelectorException = AgentBrowser.Shared.Exceptions.SelectorException;

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
    public async Task<PageNavigationResult> NavigateAsync(
        string sessionId,
        NavigateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            throw new AgentBrowserNavigationException(
                $"Invalid URL '{request.Url}'. An absolute URL is required.",
                new ArgumentException("Invalid URL.", nameof(request)));
        }

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                await handle.Page.GoToAsync(request.Url, new NavigationOptions
                {
                    Timeout = request.Timeout ?? 30000,
                    WaitUntil = [MapWaitUntil(request.WaitUntil)]
                }).ConfigureAwait(false);

                var title = await handle.Page.GetTitleAsync().ConfigureAwait(false);
                var finalUrl = handle.Page.Url;

                var tab = handle.Session.Tabs[0];
                tab.Url = finalUrl;
                tab.Title = title;
                tab.Status = TabStatus.Loaded;
                handle.Session.Status = SessionStatus.Active;
                handle.Session.LastAccessedAt = DateTime.UtcNow;

                return new PageNavigationResult(finalUrl, title);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AgentBrowserNavigationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserNavigationException(
                    $"Navigation to '{request.Url}' failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await handle.Page.GetContentAsync().ConfigureAwait(false);
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> GetContentTextAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await handle.Page
                .EvaluateExpressionAsync<string>("document.body?.innerText ?? ''")
                .ConfigureAwait(false);
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public Task<string> ScreenshotAsync(string sessionId, string tabId, object options)
        => throw new NotSupportedException("Screenshots are implemented in Task 6.");

    /// <inheritdoc />
    public async Task<T> EvaluateExpressionAsync<T>(
        string sessionId,
        string script,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(script);

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                return await handle.Page.EvaluateExpressionAsync<T>(script).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserScriptExecutionException(
                    $"JavaScript evaluation failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClickAsync(
        string sessionId,
        string selector,
        ClickOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(selector);

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                await handle.Page.ClickAsync(selector, new PuppeteerSharp.Input.ClickOptions
                {
                    Delay = options.Delay,
                    Button = MapMouseButton(options.Button),
                    Count = options.ClickCount
                }).ConfigureAwait(false);

                MarkTabInteractive(handle);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserSelectorException(
                    $"Clicking '{selector}' failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task TypeAsync(
        string sessionId,
        string selector,
        string text,
        int delay,
        bool clearFirst,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(selector);

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                if (clearFirst)
                {
                    await handle.Page.EvaluateFunctionAsync<object>(
                        "selector => { const el = document.querySelector(selector); if (el) el.value = ''; }",
                        selector).ConfigureAwait(false);
                }

                await handle.Page.TypeAsync(selector, text ?? string.Empty,
                    new PuppeteerSharp.Input.TypeOptions { Delay = delay })
                    .ConfigureAwait(false);

                MarkTabInteractive(handle);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserSelectorException(
                    $"Typing into '{selector}' failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task WaitForSelectorAsync(
        string sessionId,
        string selector,
        int timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(selector);

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                await handle.Page.WaitForSelectorAsync(selector,
                    new WaitForSelectorOptions { Timeout = timeout })
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserSelectorException(
                    $"Waiting for '{selector}' failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

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

    private static void MarkTabInteractive(SessionHandle handle)
    {
        handle.Session.LastAccessedAt = DateTime.UtcNow;
        if (handle.Session.Tabs.Count > 0)
        {
            handle.Session.Tabs[0].Status = TabStatus.Loaded;
        }
    }

    private static PuppeteerSharp.Input.MouseButton MapMouseButton(int button) => button switch
    {
        1 => PuppeteerSharp.Input.MouseButton.Middle,
        2 => PuppeteerSharp.Input.MouseButton.Right,
        _ => PuppeteerSharp.Input.MouseButton.Left
    };

    private SessionHandle GetHandle(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var handle))
        {
            throw new SessionNotFoundException(sessionId);
        }

        handle.Session.LastAccessedAt = DateTime.UtcNow;
        return handle;
    }

    private static WaitUntilNavigation MapWaitUntil(WaitUntil? waitUntil) => waitUntil switch
    {
        WaitUntil.DOMContentLoaded => WaitUntilNavigation.DOMContentLoaded,
        WaitUntil.NetworkIdle0 => WaitUntilNavigation.Networkidle0,
        WaitUntil.NetworkIdle2 => WaitUntilNavigation.Networkidle2,
        _ => WaitUntilNavigation.Load
    };

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

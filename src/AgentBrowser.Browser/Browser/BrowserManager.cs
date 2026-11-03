using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using AgentBrowserNavigationException = AgentBrowser.Shared.Exceptions.NavigationException;
using AgentBrowserScriptExecutionException = AgentBrowser.Shared.Exceptions.ScriptExecutionException;
using AgentBrowserSelectorException = AgentBrowser.Shared.Exceptions.SelectorException;
using AgentBrowserScreenshotType = AgentBrowser.Shared.Models.ScreenshotType;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// PuppeteerSharp-backed browser session manager.
/// </summary>
public sealed class BrowserManager : IBrowserManager
{
    private static readonly DateTimeOffset ProcessStartedAt = DateTimeOffset.UtcNow;

    private sealed class SessionHandle : IAsyncDisposable
    {
        public required Session Session { get; init; }
        public required IBrowser Browser { get; init; }
        public required IPage Page { get; init; }
        public SemaphoreSlim Gate { get; } = new(1, 1);
        public List<ConsoleLogEntry> ConsoleLogs { get; } = new();
        public List<PageErrorEntry> PageErrors { get; } = new();

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
                Args = options.Args?.ToArray() ?? Array.Empty<string>()
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
            SubscribeToPageEvents(handle, page, sessionId);

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
    public async Task<DiagnosticsResponse> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var path = await _engine.ResolveChromiumExecutablePathAsync(cancellationToken).ConfigureAwait(false);
        var sessions = _sessions.Values.Select(handle => handle.Session.Id).OrderBy(id => id, StringComparer.Ordinal).ToList();

        return new DiagnosticsResponse
        {
            UptimeSeconds = (DateTimeOffset.UtcNow - ProcessStartedAt).TotalSeconds,
            SessionCount = sessions.Count,
            ActiveSessionIds = sessions,
            ChromiumExecutablePath = path,
            IsChromiumAvailable = !string.IsNullOrWhiteSpace(path) && File.Exists(path)
        };
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ConsoleLogEntry>> GetConsoleLogsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var handle = GetHandle(sessionId);
        lock (handle.ConsoleLogs)
        {
            return Task.FromResult<IReadOnlyList<ConsoleLogEntry>>(handle.ConsoleLogs.ToList());
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PageErrorEntry>> GetPageErrorsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var handle = GetHandle(sessionId);
        lock (handle.PageErrors)
        {
            return Task.FromResult<IReadOnlyList<PageErrorEntry>>(handle.PageErrors.ToList());
        }
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

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            throw new AgentBrowserNavigationException(
                $"Invalid URL '{request.Url}'. An absolute URL is required.",
                new ArgumentException("Invalid URL.", nameof(request)));
        }

        var blockPrivateNetworks = bool.TryParse(
            Environment.GetEnvironmentVariable("AGENTBROWSER_BLOCK_PRIVATE_NETWORKS"),
            out var parsedBlock) && parsedBlock;

        if (!SafetyPolicy.IsAllowedNavigation(uri, out var safetyReason,
            blockPrivateNetworks: blockPrivateNetworks))
        {
            throw new AgentBrowserNavigationException(
                safetyReason,
                new InvalidOperationException(safetyReason));
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
            return await GetBodyTextAsync(handle).ConfigureAwait(false);
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> GetContentAsync(string sessionId, ContentFormat format, CancellationToken cancellationToken = default)
    {
        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return format switch
            {
                ContentFormat.Html => await handle.Page.GetContentAsync().ConfigureAwait(false),
                ContentFormat.Markdown => await handle.Page
                    .EvaluateExpressionAsync<string>(MarkdownScript)
                    .ConfigureAwait(false),
                ContentFormat.Text => await GetBodyTextAsync(handle).ConfigureAwait(false),
                _ => throw new ArgumentException($"Unsupported content format '{format}'.", nameof(format))
            };
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> EvaluateJsonFunctionAsync(
        string sessionId,
        string function,
        object[]? args = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(function);

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                var element = await handle.Page
                    .EvaluateFunctionAsync<JsonElement>(function, args ?? Array.Empty<object>())
                    .ConfigureAwait(false);
                return element.GetRawText();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserScriptExecutionException(
                    $"JavaScript function evaluation failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> ScreenshotAsync(
        string sessionId,
        AgentBrowser.Shared.Models.ScreenshotOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var format = ParseScreenshotFormat(options.Format);
        var isElement = options.Type == AgentBrowserScreenshotType.Element;
        var fullPage = options.Type == AgentBrowserScreenshotType.FullPage || options.FullPage;

        if (isElement && string.IsNullOrWhiteSpace(options.Selector))
        {
            throw new ArgumentException("Selector is required for element screenshots.", nameof(options));
        }

        var handle = GetHandle(sessionId);
        await handle.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                if (isElement)
                {
                    var element = await handle.Page.QuerySelectorAsync(options.Selector!)
                        .ConfigureAwait(false);
                    if (element is null)
                    {
                        throw new AgentBrowserSelectorException(
                            $"No element matches '{options.Selector}'.",
                            new ArgumentException("Selector not found.", nameof(options)));
                    }

                    var elementBytes = await element.ScreenshotDataAsync(new ElementScreenshotOptions
                    {
                        Type = format,
                        Quality = format == PuppeteerSharp.ScreenshotType.Png ? null : options.Quality
                    }).ConfigureAwait(false);
                    EnsureScreenshotSize(elementBytes, options);
                    return elementBytes;
                }

                var pageBytes = await handle.Page.ScreenshotDataAsync(new PuppeteerSharp.ScreenshotOptions
                {
                    Type = format,
                    FullPage = fullPage,
                    Quality = format == PuppeteerSharp.ScreenshotType.Png ? null : options.Quality
                }).ConfigureAwait(false);
                EnsureScreenshotSize(pageBytes, options);
                return pageBytes;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AgentBrowserSelectorException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AgentBrowserScriptExecutionException(
                    $"Screenshot capture failed: {ex.Message}", ex);
            }
        }
        finally
        {
            handle.Gate.Release();
        }
    }

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

    private void SubscribeToPageEvents(SessionHandle handle, IPage page, string sessionId)
    {
        page.Console += (_, args) =>
        {
            var entry = new ConsoleLogEntry(args.Message.Type.ToString(), args.Message.Text, DateTimeOffset.UtcNow);
            lock (handle.ConsoleLogs)
            {
                handle.ConsoleLogs.Add(entry);
                TrimBuffer(handle.ConsoleLogs, 200);
            }

            PublishEvent(new BrowserConsoleEvent(sessionId, entry.Level, entry.Message, entry.Timestamp));
        };

        page.Load += (_, _) => PublishEvent(new BrowserLoadCompleteEvent(
            sessionId,
            page.Url,
            DateTimeOffset.UtcNow));

        page.PageError += (_, args) =>
        {
            var entry = new PageErrorEntry(args.Message, DateTimeOffset.UtcNow);
            lock (handle.PageErrors)
            {
                handle.PageErrors.Add(entry);
                TrimBuffer(handle.PageErrors, 200);
            }

            PublishEvent(new BrowserErrorEvent(sessionId, entry.Message, entry.Timestamp));
        };
    }

    private static void TrimBuffer<T>(List<T> buffer, int max)
    {
        if (buffer.Count <= max)
        {
            return;
        }

        buffer.RemoveRange(0, buffer.Count - max);
    }

    private void PublishEvent(BrowserEvent browserEvent)
    {
        _ = PublishEventAsync(browserEvent);
    }

    private async Task PublishEventAsync(BrowserEvent browserEvent)
    {
        try
        {
            await _eventSink.PublishAsync(browserEvent).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish browser event {EventType}", browserEvent.GetType().Name);
        }
    }

    private const string MarkdownScript = """
        (() => {
            const chunks = [];
            const clean = value => (value || '').replace(/\s+/g, ' ').trim();
            for (const element of document.body?.querySelectorAll('h1, h2, h3, p, li, a') ?? []) {
                const tag = element.tagName.toLowerCase();
                const text = clean(element.textContent);
                if (!text) continue;
                if (/^h[1-3]$/.test(tag)) chunks.push('#'.repeat(Number(tag.slice(1))) + ' ' + text);
                else if (tag === 'li') chunks.push('- ' + text);
                else if (tag === 'a') {
                    const href = element.href;
                    if (href) chunks.push('[' + text + '](' + href + ')');
                } else chunks.push(text);
            }
            return chunks.join('\n\n');
        })()
        """;

    private static async Task<string> GetBodyTextAsync(SessionHandle handle)
    {
        return await handle.Page
            .EvaluateExpressionAsync<string>("document.body?.innerText ?? ''")
            .ConfigureAwait(false);
    }

    private static void EnsureScreenshotSize(byte[] bytes, AgentBrowser.Shared.Models.ScreenshotOptions options)
    {
        if (options.MaxBytes is > 0 && bytes.LongLength > options.MaxBytes.Value)
        {
            throw new AgentBrowser.Shared.Exceptions.RequestTooLargeException(
                $"Screenshot exceeded the configured maximum of {options.MaxBytes} bytes.");
        }
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

    private static PuppeteerSharp.ScreenshotType ParseScreenshotFormat(string? format) => format?.ToLowerInvariant() switch
    {
        "png" or null or "" => PuppeteerSharp.ScreenshotType.Png,
        "jpeg" or "jpg" => PuppeteerSharp.ScreenshotType.Jpeg,
        "webp" => PuppeteerSharp.ScreenshotType.Webp,
        _ => throw new ArgumentException(
            $"Unsupported screenshot format '{format}'. Use png, jpeg, jpg, or webp.",
            nameof(format))
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

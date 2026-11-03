using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Manages browser sessions and their single page.
/// </summary>
public interface IBrowserManager : IAsyncDisposable
{
    /// <summary>
    /// Creates a new browser session and page.
    /// </summary>
    Task<Session> CreateSessionAsync(SessionOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session by ID, updating its last-access time.
    /// </summary>
    Task<Session?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Gets all active sessions.
    /// </summary>
    Task<IReadOnlyList<Session>> GetAllSessionsAsync();

    /// <summary>
    /// Gets service diagnostics without launching a new browser.
    /// </summary>
    Task<DiagnosticsResponse> GetDiagnosticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets buffered console logs for a session.
    /// </summary>
    Task<IReadOnlyList<ConsoleLogEntry>> GetConsoleLogsAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets buffered page errors for a session.
    /// </summary>
    Task<IReadOnlyList<PageErrorEntry>> GetPageErrorsAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a session and its browser if it exists.
    /// </summary>
    Task<bool> CloseSessionAsync(string sessionId);

    /// <summary>
    /// Navigates a session to a URL.
    /// </summary>
    Task<PageNavigationResult> NavigateAsync(string sessionId, NavigateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the rendered page HTML.
    /// </summary>
    Task<string> GetHtmlAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets visible body text.
    /// </summary>
    Task<string> GetContentTextAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets page content in the requested format.
    /// </summary>
    Task<string> GetContentAsync(string sessionId, ContentFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a JavaScript expression and deserializes the result.
    /// </summary>
    Task<T> EvaluateExpressionAsync<T>(string sessionId, string script, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clicks an element.
    /// </summary>
    Task ClickAsync(string sessionId, string selector, ClickOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Types text into an element.
    /// </summary>
    Task TypeAsync(string sessionId, string selector, string text, int delay, bool clearFirst, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a selector to appear.
    /// </summary>
    Task WaitForSelectorAsync(string sessionId, string selector, int timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a JavaScript function and returns its result as JSON.
    /// </summary>
    Task<string> EvaluateJsonFunctionAsync(string sessionId, string function, object[]? args = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures a screenshot as image bytes.
    /// </summary>
    Task<byte[]> ScreenshotAsync(string sessionId, ScreenshotOptions options, CancellationToken cancellationToken = default);
}

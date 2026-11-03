using AgentBrowser.Shared.Models;

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
    /// Closes a session and its browser if it exists.
    /// </summary>
    Task<bool> CloseSessionAsync(string sessionId);

    // Temporary compatibility members. Tasks 4-6 replace these with typed methods.
    Task<object> NavigateAsync(string sessionId, string tabId, string url, object? options = null);
    Task<string> ScreenshotAsync(string sessionId, string tabId, object options);
    Task<object> EvaluateExpressionAsync(string sessionId, string tabId, string script);
    Task ClickAsync(string sessionId, string tabId, string selector, object? options = null);
    Task TypeAsync(string sessionId, string tabId, string selector, string text, int delay = 0);
}

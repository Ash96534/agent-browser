using AgentBrowser.Shared.Models;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Interface for managing browser sessions and tabs
/// </summary>
public interface IBrowserManager
{
    /// <summary>
    /// Creates a new browser session
    /// </summary>
    /// <param name="options">Session configuration options</param>
    /// <returns>The created session</returns>
    Task<Session> CreateSessionAsync(SessionOptions options);

    /// <summary>
    /// Gets a session by ID
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <returns>The session if found, null otherwise</returns>
    Task<Session?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Closes a session
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <returns>True if session was closed successfully</returns>
    Task<bool> CloseSessionAsync(string sessionId);

    /// <summary>
    /// Gets all active sessions
    /// </summary>
    /// <returns>Collection of all sessions</returns>
    Task<IEnumerable<Session>> GetAllSessionsAsync();

    /// <summary>
    /// Navigates a tab to a URL
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="tabId">The tab identifier</param>
    /// <param name="url">The URL to navigate to</param>
    /// <param name="options">Navigation options</param>
    /// <returns>Navigation response</returns>
    Task<object> NavigateAsync(string sessionId, string tabId, string url, object? options = null);

    /// <summary>
    /// Takes a screenshot of a tab
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="tabId">The tab identifier</param>
    /// <param name="options">Screenshot options</param>
    /// <returns>Base64 encoded screenshot</returns>
    Task<string> ScreenshotAsync(string sessionId, string tabId, object options);

    /// <summary>
    /// Executes JavaScript in a tab
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="tabId">The tab identifier</param>
    /// <param name="script">The JavaScript to execute</param>
    /// <returns>The result of the JavaScript execution</returns>
    Task<object> EvaluateExpressionAsync(string sessionId, string tabId, string script);

    /// <summary>
    /// Clicks an element in a tab
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="tabId">The tab identifier</param>
    /// <param name="selector">CSS selector for the element</param>
    /// <param name="options">Click options</param>
    /// <returns>Task</returns>
    Task ClickAsync(string sessionId, string tabId, string selector, object? options = null);

    /// <summary>
    /// Types text into an element in a tab
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="tabId">The tab identifier</param>
    /// <param name="selector">CSS selector for the element</param>
    /// <param name="text">Text to type</param>
    /// <param name="delay">Delay between keystrokes</param>
    /// <returns>Task</returns>
    Task TypeAsync(string sessionId, string tabId, string selector, string text, int delay = 0);
}

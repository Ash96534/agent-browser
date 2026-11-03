using AgentBrowser.Shared.Models.Requests;

namespace AgentBrowser.Core.Services;

/// <summary>
/// DOM and JavaScript operations.
/// </summary>
public interface IDomService
{
    /// <summary>
    /// Clicks an element.
    /// </summary>
    Task ClickAsync(string sessionId, ClickRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Types text into an element.
    /// </summary>
    Task TypeAsync(string sessionId, TypeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes JavaScript and returns the deserialized result.
    /// </summary>
    Task<T> ExecuteJsAsync<T>(string sessionId, ExecuteJsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a selector to appear.
    /// </summary>
    Task WaitForSelectorAsync(string sessionId, string selector, int timeout = 30000, CancellationToken cancellationToken = default);
}

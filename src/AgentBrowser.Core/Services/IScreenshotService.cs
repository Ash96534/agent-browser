using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Core.Services;

/// <summary>
/// Screenshot operations.
/// </summary>
public interface IScreenshotService
{
    /// <summary>
    /// Captures a screenshot and returns base64 image data.
    /// </summary>
    Task<ScreenshotResponse> CaptureAsync(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures a screenshot and returns base64 image data.
    /// </summary>
    Task<string> CaptureBase64Async(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default);
}

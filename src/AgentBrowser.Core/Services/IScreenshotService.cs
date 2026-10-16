using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Core.Services;

public interface IScreenshotService
{
    Task<ScreenshotResponse> CaptureAsync(string sessionId, string tabId, ScreenshotRequest request);
    Task<string> CaptureBase64Async(string sessionId, string tabId, ScreenshotRequest request);
}
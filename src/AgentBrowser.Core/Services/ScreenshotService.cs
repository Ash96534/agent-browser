using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

public class ScreenshotService : IScreenshotService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<ScreenshotService> _logger;

    public ScreenshotService(IBrowserManager browserManager, ILogger<ScreenshotService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<ScreenshotResponse> CaptureAsync(string sessionId, string tabId, ScreenshotRequest request)
    {
        var base64 = await _browserManager.ScreenshotAsync(sessionId, tabId, request);
        
        _logger.LogInformation("Captured screenshot for session {SessionId}", sessionId);

        return new ScreenshotResponse
        {
            ImageBase64 = base64,
            Format = request.Format
        };
    }

    public async Task<string> CaptureBase64Async(string sessionId, string tabId, ScreenshotRequest request)
    {
        var base64 = await _browserManager.ScreenshotAsync(sessionId, tabId, request);
        
        _logger.LogInformation("Captured base64 screenshot for session {SessionId}", sessionId);

        return base64;
    }
}

using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Screenshot endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}/screenshot")]
public sealed class ScreenshotController : ControllerBase
{
    private readonly IScreenshotService _screenshotService;

    /// <summary>
    /// Creates a screenshot controller.
    /// </summary>
    public ScreenshotController(IScreenshotService screenshotService)
    {
        _screenshotService = screenshotService;
    }

    /// <summary>
    /// Captures a screenshot.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> Capture(string sessionId,
        [FromQuery] string type = "viewport",
        [FromQuery] string? selector = null,
        [FromQuery] bool fullPage = false,
        [FromQuery] string format = "png",
        [FromQuery] int? quality = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(type, selector, fullPage, format, quality);
        var result = await _screenshotService.CaptureAsync(sessionId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Captures a screenshot as base64.
    /// </summary>
    [HttpGet("base64")]
    public async Task<ActionResult> CaptureBase64(string sessionId,
        [FromQuery] string type = "viewport",
        [FromQuery] string? selector = null,
        [FromQuery] bool fullPage = false,
        [FromQuery] string format = "png",
        [FromQuery] int? quality = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(type, selector, fullPage, format, quality);
        var base64 = await _screenshotService.CaptureBase64Async(sessionId, request, cancellationToken);
        return Ok(new { base64 });
    }

    private static ScreenshotRequest CreateRequest(string type, string? selector, bool fullPage, string format, int? quality)
        => new()
        {
            Type = Enum.Parse<ScreenshotType>(type, ignoreCase: true),
            Selector = selector,
            FullPage = fullPage,
            Format = format,
            Quality = quality
        };
}

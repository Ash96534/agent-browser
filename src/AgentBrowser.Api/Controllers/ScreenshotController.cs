using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("session/{sessionId}/screenshot")]
public class ScreenshotController : ControllerBase
{
    private readonly IScreenshotService _screenshotService;

    public ScreenshotController(IScreenshotService screenshotService)
    {
        _screenshotService = screenshotService;
    }

    /// <summary>
    /// Capture screenshot
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="type">Screenshot type (viewport, element, fullPage)</param>
    /// <param name="selector">Element selector (for element type)</param>
    /// <param name="fullPage">Full page flag</param>
    /// <param name="format">Image format (png, jpeg)</param>
    /// <param name="quality">Quality for JPEG (0-100)</param>
    /// <returns>Base64 screenshot</returns>
    [HttpGet]
    public async Task<ActionResult> Capture(string sessionId, 
        [FromQuery] string type = "viewport",
        [FromQuery] string? selector = null,
        [FromQuery] bool fullPage = false,
        [FromQuery] string format = "png",
        [FromQuery] int? quality = null)
    {
        try
        {
            var request = new ScreenshotRequest
            {
                Type = Enum.Parse<ScreenshotType>(type, ignoreCase: true),
                Selector = selector,
                FullPage = fullPage,
                Format = format,
                Quality = quality
            };

            var result = await _screenshotService.CaptureAsync(sessionId, sessionId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Capture screenshot as base64
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="type">Screenshot type</param>
    /// <param name="selector">Element selector</param>
    /// <param name="fullPage">Full page flag</param>
    /// <param name="format">Image format</param>
    /// <param name="quality">Quality for JPEG</param>
    /// <returns>Base64 screenshot</returns>
    [HttpGet("base64")]
    public async Task<ActionResult> CaptureBase64(string sessionId,
        [FromQuery] string type = "viewport",
        [FromQuery] string? selector = null,
        [FromQuery] bool fullPage = false,
        [FromQuery] string format = "png",
        [FromQuery] int? quality = null)
    {
        try
        {
            var request = new ScreenshotRequest
            {
                Type = Enum.Parse<ScreenshotType>(type, ignoreCase: true),
                Selector = selector,
                FullPage = fullPage,
                Format = format,
                Quality = quality
            };

            var base64 = await _screenshotService.CaptureBase64Async(sessionId, sessionId, request);
            return Ok(new { base64 = base64 });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

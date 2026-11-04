using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Advanced agent-oriented page inspection and safe interaction endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}")]
public sealed class AdvancedController : ControllerBase
{
    private readonly IBrowserManager _browserManager;

    /// <summary>
    /// Creates an advanced controller.
    /// </summary>
    public AdvancedController(IBrowserManager browserManager)
    {
        _browserManager = browserManager;
    }

    /// <summary>
    /// Lists visible, actionable elements.
    /// </summary>
    [HttpGet("clickable-elements")]
    public async Task<ActionResult> ClickableElements(string sessionId, CancellationToken cancellationToken)
    {
        var elements = await _browserManager.GetClickableElementsAsync(sessionId, cancellationToken);
        return Ok(elements);
    }

    /// <summary>
    /// Fills fields by visible label text.
    /// </summary>
    [HttpPost("dom/fill")]
    public async Task<ActionResult> FillByLabel(string sessionId,
        [FromBody] FillByLabelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _browserManager.FillByLabelsAsync(sessionId, request.Fields, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Compares the current viewport screenshot to the previous screenshot.
    /// </summary>
    [HttpGet("visual-diff")]
    public async Task<ActionResult> VisualDiff(string sessionId, CancellationToken cancellationToken)
    {
        var diff = await _browserManager.GetVisualDiffAsync(sessionId, cancellationToken);
        return Ok(diff);
    }

    /// <summary>
    /// Clicks an element only with explicit confirmation.
    /// </summary>
    [HttpPost("click-safe")]
    public async Task<ActionResult> ClickSafe(string sessionId,
        [FromBody] ClickSafeRequest request,
        CancellationToken cancellationToken)
    {
        await _browserManager.ClickSafeAsync(sessionId, request.Selector, request.Confirm, cancellationToken);
        return Ok(new { success = true });
    }
}

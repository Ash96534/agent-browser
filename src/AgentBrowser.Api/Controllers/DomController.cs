using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// DOM and JavaScript endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}/dom")]
public sealed class DomController : ControllerBase
{
    private readonly IDomService _domService;

    /// <summary>
    /// Creates a DOM controller.
    /// </summary>
    public DomController(IDomService domService)
    {
        _domService = domService;
    }

    /// <summary>
    /// Clicks an element.
    /// </summary>
    [HttpPost("click")]
    public async Task<ActionResult> Click(string sessionId, [FromBody] ClickRequest request,
        CancellationToken cancellationToken)
    {
        await _domService.ClickAsync(sessionId, request, cancellationToken);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Types text into an element.
    /// </summary>
    [HttpPost("type")]
    public async Task<ActionResult> Type(string sessionId, [FromBody] TypeRequest request,
        CancellationToken cancellationToken)
    {
        await _domService.TypeAsync(sessionId, request, cancellationToken);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Executes JavaScript.
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteJs(string sessionId, [FromBody] ExecuteJsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _domService.ExecuteJsAsync<JsonElement>(sessionId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Waits for a selector to appear.
    /// </summary>
    [HttpGet("wait")]
    public async Task<ActionResult> WaitForSelector(string sessionId, [FromQuery] string selector,
        [FromQuery] int timeout = 30000, CancellationToken cancellationToken = default)
    {
        await _domService.WaitForSelectorAsync(sessionId, selector, timeout, cancellationToken);
        return Ok(new { success = true });
    }
}

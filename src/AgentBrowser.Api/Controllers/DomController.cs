using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("session/{sessionId}/dom")]
public class DomController : ControllerBase
{
    private readonly IDomService _domService;

    public DomController(IDomService domService)
    {
        _domService = domService;
    }

    /// <summary>
    /// Click element
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Click request</param>
    /// <returns>Success status</returns>
    [HttpPost("click")]
    public async Task<ActionResult> Click(string sessionId, [FromBody] ClickRequest request)
    {
        try
        {
            await _domService.ClickAsync(sessionId, sessionId, request);
            return Ok(new { success = true });
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
    /// Type text into element
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Type request</param>
    /// <returns>Success status</returns>
    [HttpPost("type")]
    public async Task<ActionResult> Type(string sessionId, [FromBody] TypeRequest request)
    {
        try
        {
            await _domService.TypeAsync(sessionId, sessionId, request);
            return Ok(new { success = true });
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
    /// Execute JavaScript
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Execute JS request</param>
    /// <returns>JS result</returns>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteJs<T>(string sessionId, [FromBody] ExecuteJsRequest request)
    {
        try
        {
            var result = await _domService.ExecuteJsAsync<T>(sessionId, sessionId, request);
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
    /// Wait for selector
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="selector">CSS selector</param>
    /// <param name="timeout">Timeout in ms (default: 30000)</param>
    /// <returns>Success status</returns>
    [HttpGet("wait")]
    public async Task<ActionResult> WaitForSelector(string sessionId, [FromQuery] string selector, [FromQuery] int timeout = 30000)
    {
        try
        {
            await _domService.WaitForSelectorAsync(sessionId, sessionId, selector, timeout);
            return Ok(new { success = true });
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

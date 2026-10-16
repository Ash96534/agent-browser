using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("session/{sessionId}/navigate")]
public class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public NavigationController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    /// <summary>
    /// Navigate to URL
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="request">Navigation request</param>
    /// <returns>Navigation result</returns>
    [HttpPost]
    public async Task<ActionResult> Navigate(string sessionId, [FromBody] NavigateRequest request)
    {
        try
        {
            var result = await _navigationService.NavigateAsync(sessionId, sessionId, request);
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
    /// Get rendered HTML
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>HTML content</returns>
    [HttpGet("/session/{sessionId}/html")]
    public async Task<ActionResult> GetHtml(string sessionId)
    {
        try
        {
            var result = await _navigationService.GetHtmlAsync(sessionId, sessionId);
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
    /// Get page content
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Content result</returns>
    [HttpGet("/session/{sessionId}/content")]
    public async Task<ActionResult> GetContent(string sessionId)
    {
        try
        {
            var result = await _navigationService.GetContentAsync(sessionId, sessionId);
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
}

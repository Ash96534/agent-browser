using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Navigation and content endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}/navigate")]
public sealed class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Creates a navigation controller.
    /// </summary>
    public NavigationController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    /// <summary>
    /// Navigates a session to a URL.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Navigate(string sessionId, [FromBody] NavigateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.NavigateAsync(sessionId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the rendered HTML.
    /// </summary>
    [HttpGet("/session/{sessionId}/html")]
    public async Task<ActionResult> GetHtml(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _navigationService.GetHtmlAsync(sessionId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the visible body text.
    /// </summary>
    [HttpGet("/session/{sessionId}/content")]
    public async Task<ActionResult> GetContent(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _navigationService.GetContentAsync(sessionId, cancellationToken);
        return Ok(result);
    }
}

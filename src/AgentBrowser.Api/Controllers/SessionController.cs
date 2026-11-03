using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowserSessionOptions = AgentBrowser.Shared.Models.SessionOptions;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Session management endpoints.
/// </summary>
[ApiController]
[Route("session")]
public sealed class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    /// <summary>
    /// Creates a session controller.
    /// </summary>
    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Creates a new browser session.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateSession([FromBody] AgentBrowserSessionOptions options, CancellationToken cancellationToken)
    {
        var session = await _sessionService.CreateSessionAsync(options);
        return Ok(session);
    }

    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetSession(string id)
    {
        var session = await _sessionService.GetSessionAsync(id);
        return session is null ? NotFound(new { error = "Session not found" }) : Ok(session);
    }

    /// <summary>
    /// Closes a session.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> CloseSession(string id)
    {
        var success = await _sessionService.CloseSessionAsync(id);
        return success ? Ok(new { success = true }) : NotFound(new { error = "Session not found" });
    }

    /// <summary>
    /// Lists active sessions.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllSessions()
    {
        var sessions = await _sessionService.GetAllSessionsAsync();
        return Ok(sessions);
    }
}

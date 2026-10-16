using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("session")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Create a new browser session
    /// </summary>
    /// <param name="options">Session configuration options</param>
    /// <returns>The created session</returns>
    [HttpPost]
    public async Task<ActionResult> CreateSession([FromBody] AgentBrowser.Shared.Models.SessionOptions options)
    {
        try
        {
            var session = await _sessionService.CreateSessionAsync(options);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get session state
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <returns>Session state</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetSession(string id)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id);
            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Close a session
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> CloseSession(string id)
    {
        try
        {
            var success = await _sessionService.CloseSessionAsync(id);
            if (!success)
            {
                return NotFound(new { error = "Session not found" });
            }
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all active sessions
    /// </summary>
    /// <returns>List of all sessions</returns>
    [HttpGet]
    public async Task<ActionResult> GetAllSessions()
    {
        try
        {
            var sessions = await _sessionService.GetAllSessionsAsync();
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

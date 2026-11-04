using AgentBrowser.Browser.Browser;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Per-session browser telemetry endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IBrowserManager _browserManager;

    /// <summary>
    /// Creates a telemetry controller.
    /// </summary>
    public TelemetryController(IBrowserManager browserManager)
    {
        _browserManager = browserManager;
    }

    /// <summary>
    /// Gets recent console logs.
    /// </summary>
    [HttpGet("console")]
    public async Task<ActionResult> GetConsoleLogs(string sessionId, CancellationToken cancellationToken)
    {
        var logs = await _browserManager.GetConsoleLogsAsync(sessionId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Gets the crash or closed state of a session.
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> GetSessionHealth(string sessionId, CancellationToken cancellationToken)
    {
        var health = await _browserManager.GetSessionHealthAsync(sessionId, cancellationToken);
        return Ok(health);
    }

    /// <summary>
    /// Recycles a session's browser and page, preserving the session ID and last URL.
    /// </summary>
    [HttpPost("restart")]
    public async Task<ActionResult> RestartSession(string sessionId, CancellationToken cancellationToken)
    {
        var restarted = await _browserManager.RestartSessionAsync(sessionId, cancellationToken);
        return restarted ? Ok(new { restarted = true }) : NotFound(new { restarted = false });
    }

    /// <summary>
    /// Gets recent page errors.
    /// </summary>
    [HttpGet("errors")]
    public async Task<ActionResult> GetPageErrors(string sessionId, CancellationToken cancellationToken)
    {
        var errors = await _browserManager.GetPageErrorsAsync(sessionId, cancellationToken);
        return Ok(errors);
    }
}

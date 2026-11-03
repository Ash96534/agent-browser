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
    /// Gets recent page errors.
    /// </summary>
    [HttpGet("errors")]
    public async Task<ActionResult> GetPageErrors(string sessionId, CancellationToken cancellationToken)
    {
        var errors = await _browserManager.GetPageErrorsAsync(sessionId, cancellationToken);
        return Ok(errors);
    }
}

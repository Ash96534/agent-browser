using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Service metadata, version, and capability endpoints.
/// </summary>
[ApiController]
public sealed class MetaController : ControllerBase
{
    private readonly IBrowserManager _browserManager;

    /// <summary>
    /// Creates a metadata controller.
    /// </summary>
    public MetaController(IBrowserManager browserManager)
    {
        _browserManager = browserManager;
    }

    /// <summary>
    /// Gets runtime diagnostics without launching a new browser.
    /// </summary>
    [HttpGet("/diagnostics")]
    public async Task<ActionResult<DiagnosticsResponse>> Diagnostics(CancellationToken cancellationToken)
    {
        var diagnostics = await _browserManager.GetDiagnosticsAsync(cancellationToken);
        return Ok(diagnostics);
    }

    /// <summary>
    /// Gets the API feature manifest.
    /// </summary>
    [HttpGet("/capabilities")]
    public async Task<ActionResult<object>> Capabilities(CancellationToken cancellationToken)
    {
        var diagnostics = await _browserManager.GetDiagnosticsAsync(cancellationToken);

        return Ok(new
        {
            service = diagnostics.Service,
            apiVersion = diagnostics.ApiVersion,
            schemaVersion = diagnostics.SchemaVersion,
            chromium = new
            {
                available = diagnostics.IsChromiumAvailable,
                executablePath = diagnostics.ChromiumExecutablePath
            },
            features = new
            {
                multiTab = false,
                networkInterception = false,
                domMutationEvents = false,
                persistence = true,
                streaming = true,
                apiKeyAuthentication = Environment.GetEnvironmentVariable("AGENTBROWSER_API_KEY") is { Length: > 0 },
                requestSizeLimit = true,
                correlationIds = true,
                sessionRestart = true,
                invoiceExtraction = true
            },
            endpoints = new[]
            {
                "POST /session", "GET /session", "GET /session/{id}", "DELETE /session/{id}",
                "POST /session/{id}/navigate", "GET /session/{id}/html", "GET /session/{id}/content",
                "POST /session/{id}/dom/click", "POST /session/{id}/dom/type", "POST /session/{id}/dom/execute",
                "GET /session/{id}/dom/wait", "POST /session/{id}/dom/fill",
                "GET /session/{id}/screenshot", "GET /session/{id}/screenshot/base64",
                "GET /session/{id}/extract/metadata", "GET /session/{id}/extract/prices",
                "GET /session/{id}/extract/images", "GET /session/{id}/extract/forms", "GET /session/{id}/extract/links",
                "GET /session/{id}/extract/invoice",
                "GET /session/{id}/console", "GET /session/{id}/errors",
                "GET /session/{id}/health", "POST /session/{id}/restart",
                "GET /session/{id}/clickable-elements", "GET /session/{id}/visual-diff",
                "POST /session/{id}/click-safe",
                "GET /health", "GET /capabilities", "GET /diagnostics"
            },
            limitations = new[]
            {
                "One active page per session",
                "No network interception or request blocking",
                "No DOM mutation events",
                "Session state stays in memory"
            }
        });
    }
}

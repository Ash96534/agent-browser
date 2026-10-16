using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    public ActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "AgentBrowser"
        });
    }
}

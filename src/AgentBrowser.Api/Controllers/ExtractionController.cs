using AgentBrowser.Core.Extraction;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

[ApiController]
[Route("session/{sessionId}/extract")]
public class ExtractionController : ControllerBase
{
    private readonly ExtractionService _extractionService;

    public ExtractionController(ExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    /// <summary>
    /// Extract hidden metadata (OG tags, JSON-LD, etc.)
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Extracted metadata</returns>
    [HttpGet("metadata")]
    public async Task<ActionResult> ExtractMetadata(string sessionId)
    {
        try
        {
            var metadata = await _extractionService.ExtractMetadataAsync(sessionId, sessionId);
            return Ok(metadata);
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
    /// Extract prices from the page
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>List of prices</returns>
    [HttpGet("prices")]
    public async Task<ActionResult> ExtractPrices(string sessionId)
    {
        try
        {
            var prices = await _extractionService.ExtractPricesAsync(sessionId, sessionId);
            return Ok(prices);
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
    /// Extract images with context
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>List of images with context</returns>
    [HttpGet("images")]
    public async Task<ActionResult> ExtractImages(string sessionId)
    {
        try
        {
            var images = await _extractionService.ExtractImagesAsync(sessionId, sessionId);
            return Ok(images);
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
    /// Extract forms from the page
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>List of forms</returns>
    [HttpGet("forms")]
    public async Task<ActionResult> ExtractForms(string sessionId)
    {
        try
        {
            var forms = await _extractionService.ExtractFormsAsync(sessionId, sessionId);
            return Ok(forms);
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
    /// Extract links from the page
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>List of links</returns>
    [HttpGet("links")]
    public async Task<ActionResult> ExtractLinks(string sessionId)
    {
        try
        {
            var links = await _extractionService.ExtractLinksAsync(sessionId, sessionId);
            return Ok(links);
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

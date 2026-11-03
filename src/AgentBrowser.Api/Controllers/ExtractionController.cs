using AgentBrowser.Core.Extraction;
using Microsoft.AspNetCore.Mvc;

namespace AgentBrowser.Api.Controllers;

/// <summary>
/// Hidden-data extraction endpoints.
/// </summary>
[ApiController]
[Route("session/{sessionId}/extract")]
public sealed class ExtractionController : ControllerBase
{
    private readonly ExtractionService _extractionService;

    /// <summary>
    /// Creates an extraction controller.
    /// </summary>
    public ExtractionController(ExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    /// <summary>
    /// Extracts hidden metadata (OG tags, JSON-LD, etc.).
    /// </summary>
    [HttpGet("metadata")]
    public async Task<ActionResult> ExtractMetadata(string sessionId, CancellationToken cancellationToken)
    {
        var metadata = await _extractionService.ExtractMetadataAsync(sessionId, cancellationToken);
        return Ok(metadata);
    }

    /// <summary>
    /// Extracts prices from the page.
    /// </summary>
    [HttpGet("prices")]
    public async Task<ActionResult> ExtractPrices(string sessionId, CancellationToken cancellationToken)
    {
        var prices = await _extractionService.ExtractPricesAsync(sessionId, cancellationToken);
        return Ok(prices);
    }

    /// <summary>
    /// Extracts images with context.
    /// </summary>
    [HttpGet("images")]
    public async Task<ActionResult> ExtractImages(string sessionId, CancellationToken cancellationToken)
    {
        var images = await _extractionService.ExtractImagesAsync(sessionId, cancellationToken);
        return Ok(images);
    }

    /// <summary>
    /// Extracts forms from the page.
    /// </summary>
    [HttpGet("forms")]
    public async Task<ActionResult> ExtractForms(string sessionId, CancellationToken cancellationToken)
    {
        var forms = await _extractionService.ExtractFormsAsync(sessionId, cancellationToken);
        return Ok(forms);
    }

    /// <summary>
    /// Extracts links from the page.
    /// </summary>
    [HttpGet("links")]
    public async Task<ActionResult> ExtractLinks(string sessionId, CancellationToken cancellationToken)
    {
        var links = await _extractionService.ExtractLinksAsync(sessionId, cancellationToken);
        return Ok(links);
    }
}

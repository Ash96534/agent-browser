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
    /// Extracts a basic invoice schema from embedded structured data.
    /// </summary>
    [HttpGet("invoice")]
    public async Task<ActionResult> ExtractInvoice(string sessionId, CancellationToken cancellationToken)
    {
        var invoice = await _extractionService.ExtractInvoiceAsync(sessionId, cancellationToken);
        return Ok(invoice);
    }

    /// <summary>
    /// Extracts prices from the page.
    /// </summary>
    [HttpGet("prices")]
    public async Task<ActionResult> ExtractPrices(string sessionId,
        [FromQuery] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var prices = await _extractionService.ExtractPricesAsync(sessionId, max, cancellationToken);
        return Ok(prices);
    }

    /// <summary>
    /// Extracts images with context.
    /// </summary>
    [HttpGet("images")]
    public async Task<ActionResult> ExtractImages(string sessionId,
        [FromQuery] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var images = await _extractionService.ExtractImagesAsync(sessionId, max, cancellationToken);
        return Ok(images);
    }

    /// <summary>
    /// Extracts forms from the page.
    /// </summary>
    [HttpGet("forms")]
    public async Task<ActionResult> ExtractForms(string sessionId,
        [FromQuery] int? max = null,
        CancellationToken cancellationToken = default)
    {
        var forms = await _extractionService.ExtractFormsAsync(sessionId, max, cancellationToken);
        return Ok(forms);
    }

    /// <summary>
    /// Extracts links from the page.
    /// </summary>
    [HttpGet("links")]
    public async Task<ActionResult> ExtractLinks(string sessionId,
        [FromQuery] int? max = null,
        [FromQuery] bool sameOrigin = false,
        CancellationToken cancellationToken = default)
    {
        var links = await _extractionService.ExtractLinksAsync(sessionId, max, sameOrigin, cancellationToken);
        return Ok(links);
    }
}

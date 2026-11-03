using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

/// <summary>
/// Maps browser screenshot bytes to API response models.
/// </summary>
public sealed class ScreenshotService : IScreenshotService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<ScreenshotService> _logger;

    /// <summary>
    /// Creates a screenshot service.
    /// </summary>
    public ScreenshotService(IBrowserManager browserManager, ILogger<ScreenshotService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScreenshotResponse> CaptureAsync(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var bytes = await CaptureBytesAsync(sessionId, request, cancellationToken).ConfigureAwait(false);

        return new ScreenshotResponse
        {
            ImageBase64 = Convert.ToBase64String(bytes),
            Format = request.Format,
            Bytes = bytes.Length
        };
    }

    /// <inheritdoc />
    public async Task<string> CaptureBase64Async(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken = default)
    {
        var bytes = await CaptureBytesAsync(sessionId, request, cancellationToken).ConfigureAwait(false);
        return Convert.ToBase64String(bytes);
    }

    private async Task<byte[]> CaptureBytesAsync(string sessionId, ScreenshotRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var bytes = await _browserManager.ScreenshotAsync(sessionId, ToOptions(request), cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Captured screenshot for session {SessionId}", sessionId);
        return bytes;
    }

    private static ScreenshotOptions ToOptions(ScreenshotRequest request) => new()
    {
        Type = request.Type,
        Selector = request.Selector,
        FullPage = request.FullPage,
        Format = request.Format,
        Quality = request.Quality,
        MaxBytes = request.MaxBytes
    };
}

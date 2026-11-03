using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgentBrowser.Tests.Unit.Core;

public class ScreenshotServiceTests
{
    [Fact]
    public async Task CaptureBase64Async_ReturnsBase64EncodedBytes()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.ScreenshotAsync("session-1", It.IsAny<ScreenshotOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var service = new ScreenshotService(manager.Object, NullLogger<ScreenshotService>.Instance);
        var result = await service.CaptureBase64Async("session-1", new ScreenshotRequest());

        Assert.Equal(Convert.ToBase64String(bytes), result);
    }

    [Fact]
    public async Task CaptureAsync_MapsRequestOptionsAndFormat()
    {
        var bytes = new byte[] { 9 };
        var manager = new Mock<IBrowserManager>();
        ScreenshotOptions? captured = null;
        manager.Setup(m => m.ScreenshotAsync("session-1", It.IsAny<ScreenshotOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes)
            .Callback<string, ScreenshotOptions, CancellationToken>((_, options, _) => captured = options);

        var service = new ScreenshotService(manager.Object, NullLogger<ScreenshotService>.Instance);
        var response = await service.CaptureAsync("session-1", new ScreenshotRequest
        {
            Type = AgentBrowser.Shared.Models.ScreenshotType.FullPage,
            Format = "jpeg",
            Quality = 60,
            Selector = ".card"
        });

        Assert.Equal("jpeg", response.Format);
        Assert.Equal(Convert.ToBase64String(bytes), response.ImageBase64);
        Assert.NotNull(captured);
        Assert.Equal(AgentBrowser.Shared.Models.ScreenshotType.FullPage, captured!.Type);
        Assert.Equal("jpeg", captured.Format);
        Assert.Equal(60, captured.Quality);
        Assert.Equal(".card", captured.Selector);
    }
}

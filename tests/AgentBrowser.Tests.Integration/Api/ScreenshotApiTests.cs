using System.Net;
using System.Net.Http.Json;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public class ScreenshotApiTests
{
    [Fact]
    public async Task CaptureBase64_ReturnsBase64()
    {
        using var factory = new ApiFactory();
        var bytes = new byte[] { 1, 2, 3 };
        factory.Manager
            .Setup(m => m.ScreenshotAsync("session-1", It.IsAny<AgentBrowser.Shared.Models.ScreenshotOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1/screenshot/base64");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Base64Response>();
        Assert.Equal(Convert.ToBase64String(bytes), body!.Base64);
    }

    [Fact]
    public async Task Capture_InvalidType_ReturnsBadRequest()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/session/session-1/screenshot?type=nope");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Capture_JpegRequest_PassesFormatAndQuality()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.ScreenshotAsync("session-1", It.IsAny<AgentBrowser.Shared.Models.ScreenshotOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 4, 5 });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1/screenshot?format=jpeg&quality=70");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        factory.Manager.Verify(m => m.ScreenshotAsync("session-1",
            It.Is<AgentBrowser.Shared.Models.ScreenshotOptions>(o => o.Format == "jpeg" && o.Quality == 70),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed record Base64Response(string Base64);
}

using System.Net;
using System.Net.Http.Json;
using AgentBrowser.Shared.Models;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public class MetaApiTests
{
    [Fact]
    public async Task Diagnostics_ReturnsRuntimeInfo()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.GetDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiagnosticsResponse
            {
                SessionCount = 0,
                ActiveSessionIds = Array.Empty<string>(),
                ChromiumExecutablePath = "/tmp/chrome",
                IsChromiumAvailable = false
            });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/diagnostics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DiagnosticsResponse>();
        Assert.Equal("AgentBrowser", body!.Service);
        Assert.Equal("1.0.0", body.ApiVersion);
    }

    [Fact]
    public async Task Capabilities_ReturnsFeatureManifest()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.GetDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiagnosticsResponse
            {
                SessionCount = 0,
                ActiveSessionIds = Array.Empty<string>(),
                ChromiumExecutablePath = "/tmp/chrome",
                IsChromiumAvailable = true
            });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/capabilities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.False(root.GetProperty("features").GetProperty("multiTab").GetBoolean());
        Assert.Equal("1.0.0", root.GetProperty("apiVersion").GetString());
        Assert.Contains(root.GetProperty("endpoints").EnumerateArray(), item => item.GetString() == "GET /capabilities");
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models.Requests;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public class DomApiTests
{
    [Fact]
    public async Task Click_ReturnsSuccess()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.ClickAsync("session-1", ".btn", It.IsAny<AgentBrowser.Shared.Models.ClickOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/session-1/dom/click",
            new ClickRequest { Selector = ".btn" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Click_InvalidSelector_ReturnsBadRequest()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.ClickAsync("session-1", ".missing", It.IsAny<AgentBrowser.Shared.Models.ClickOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SelectorException("not found", new InvalidOperationException()));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/session-1/dom/click",
            new ClickRequest { Selector = ".missing" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteJs_ReturnsResult()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.EvaluateExpressionAsync<JsonElement>("session-1", "1 + 2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.SerializeToElement(3));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/session-1/dom/execute",
            new ExecuteJsRequest { Script = "1 + 2" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WaitForSelector_ReturnsSuccess()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.WaitForSelectorAsync("session-1", ".ready", 1234, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1/dom/wait?selector=.ready&timeout=1234");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        factory.Manager.Verify(m => m.WaitForSelectorAsync("session-1", ".ready", 1234, It.IsAny<CancellationToken>()), Times.Once);
    }
}

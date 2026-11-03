using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgentBrowser.Tests.Unit.Core;

public class DomServiceTests
{
    [Fact]
    public async Task ClickAsync_PassSelectorAndOptions()
    {
        var manager = new Mock<IBrowserManager>();
        var service = new DomService(manager.Object, NullLogger<DomService>.Instance);
        var request = new ClickRequest
        {
            Selector = ".btn",
            Options = new ClickOptions { Delay = 10 }
        };

        await service.ClickAsync("session-1", request);

        manager.Verify(m => m.ClickAsync("session-1", ".btn", request.Options, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TypeAsync_PassTextAndClearFirst()
    {
        var manager = new Mock<IBrowserManager>();
        var service = new DomService(manager.Object, NullLogger<DomService>.Instance);
        var request = new TypeRequest
        {
            Selector = "#email",
            Text = "hello",
            Delay = 5,
            ClearFirst = false
        };

        await service.TypeAsync("session-1", request);

        manager.Verify(m => m.TypeAsync("session-1", "#email", "hello", 5, false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteJsAsync_ReturnsTypedResult()
    {
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.EvaluateExpressionAsync<int>("session-1", "1 + 2",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = new DomService(manager.Object, NullLogger<DomService>.Instance);
        var result = await service.ExecuteJsAsync<int>("session-1",
            new ExecuteJsRequest { Script = "1 + 2" });

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task WaitForSelectorAsync_PassSelectorAndTimeout()
    {
        var manager = new Mock<IBrowserManager>();
        var service = new DomService(manager.Object, NullLogger<DomService>.Instance);

        await service.WaitForSelectorAsync("session-1", ".ready", 1234);

        manager.Verify(m => m.WaitForSelectorAsync("session-1", ".ready", 1234,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;

namespace AgentBrowser.Tests.Unit.Browser;

public class BrowserManagerDomTests
{
    [Fact]
    public async Task ClickAsync_UsesMappedMouseButtonAndDelay()
    {
        var (manager, page) = CreateManagerWithPage();
        var session = await manager.CreateSessionAsync(new SessionOptions());

        await manager.ClickAsync(session.Id, ".btn",
            new ClickOptions { Button = 1, Delay = 25, ClickCount = 2 });

        page.Verify(p => p.ClickAsync(".btn",
            It.Is<PuppeteerSharp.Input.ClickOptions>(o =>
                o.Button == PuppeteerSharp.Input.MouseButton.Middle &&
                o.Delay == 25 &&
                o.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task TypeAsync_ClearsFieldBeforeTyping()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.EvaluateFunctionAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync(new object());
        page.Setup(p => p.TypeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PuppeteerSharp.Input.TypeOptions>()))
            .Returns(Task.CompletedTask);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await manager.TypeAsync(session.Id, "#email", "hello", 5, clearFirst: true);

        page.Verify(p => p.EvaluateFunctionAsync<object>(
            It.IsAny<string>(), It.Is<object[]>(a => a.Length == 1 && (string?)a[0] == "#email")), Times.Once);
        page.Verify(p => p.TypeAsync("#email", "hello",
            It.Is<PuppeteerSharp.Input.TypeOptions>(o => o.Delay == 5)), Times.Once);
    }

    [Fact]
    public async Task TypeAsync_SkipsClearWhenClearFirstIsFalse()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.TypeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PuppeteerSharp.Input.TypeOptions>()))
            .Returns(Task.CompletedTask);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await manager.TypeAsync(session.Id, "#email", "hello", 0, clearFirst: false);

        page.Verify(p => p.EvaluateFunctionAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        page.Verify(p => p.TypeAsync("#email", "hello", It.IsAny<PuppeteerSharp.Input.TypeOptions>()), Times.Once);
    }

    [Fact]
    public async Task WaitForSelectorAsync_WhenPageTimesOut_WrapsInSelectorException()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.WaitForSelectorAsync(It.IsAny<string>(), It.IsAny<WaitForSelectorOptions>()))
            .ThrowsAsync(new TimeoutException("timeout"));

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<AgentBrowser.Shared.Exceptions.SelectorException>(
            () => manager.WaitForSelectorAsync(session.Id, ".missing", 1000));
    }

    [Fact]
    public async Task EvaluateExpressionAsync_WhenScriptThrows_WrapsInScriptExecutionException()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.EvaluateExpressionAsync<int>(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<AgentBrowser.Shared.Exceptions.ScriptExecutionException>(
            () => manager.EvaluateExpressionAsync<int>(session.Id, "throw new Error()"));
    }

    [Fact]
    public async Task EvaluateExpressionAsync_ReturnsTypedResult()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.EvaluateExpressionAsync<int>("1 + 2")).ReturnsAsync(3);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.Equal(3, await manager.EvaluateExpressionAsync<int>(session.Id, "1 + 2"));
    }

    [Fact]
    public async Task ClickAsync_UnknownSession_ThrowsSessionNotFoundException()
    {
        var (manager, _) = CreateManagerWithPage();

        await Assert.ThrowsAsync<SessionNotFoundException>(
            () => manager.ClickAsync("missing", ".btn", new ClickOptions()));
    }

    private static (BrowserManager Manager, Mock<IPage> Page) CreateManagerWithPage()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), It.IsAny<UserAgentMetadata>()))
            .Returns(Task.CompletedTask);
        page.Setup(p => p.ClickAsync(It.IsAny<string>(), It.IsAny<PuppeteerSharp.Input.ClickOptions>()))
            .Returns(Task.CompletedTask);
        page.SetupGet(p => p.Url).Returns("about:blank");

        var browser = new Mock<IBrowser>();
        browser.Setup(b => b.NewPageAsync()).ReturnsAsync(page.Object);
        browser.Setup(b => b.CloseAsync()).Returns(Task.CompletedTask);

        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(browser.Object);

        var manager = new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);
        return (manager, page);
    }
}

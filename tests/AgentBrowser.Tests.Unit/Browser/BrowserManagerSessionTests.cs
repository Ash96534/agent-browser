using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;

namespace AgentBrowser.Tests.Unit.Browser;

public class BrowserManagerSessionTests
{
    [Fact]
    public async Task CreateSessionAsync_LaunchesBrowserAndStoresSession()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), It.IsAny<UserAgentMetadata>()))
            .Returns(Task.CompletedTask);

        var browser = new Mock<IBrowser>();
        browser.Setup(b => b.NewPageAsync()).ReturnsAsync(page.Object);

        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(browser.Object);

        var manager = new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.NotEmpty(session.Id);
        Assert.Single(session.Tabs);
        Assert.Equal(SessionStatus.Active, session.Status);
        browser.Verify(b => b.NewPageAsync(), Times.Once);
        page.Verify(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetSessionAsync_UnknownId_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(await manager.GetSessionAsync("missing"));
    }

    [Fact]
    public async Task CloseSessionAsync_ClosesBrowserAndRemovesSession()
    {
        var (manager, browser, _) = CreateManagerWithBrowser();
        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.True(await manager.CloseSessionAsync(session.Id));
        Assert.Null(await manager.GetSessionAsync(session.Id));
        browser.Verify(b => b.CloseAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseSessionAsync_UnknownId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(await manager.CloseSessionAsync("missing"));
    }

    [Fact]
    public async Task CreateSessionAsync_WhenLaunchFails_ThrowsBrowserLaunchException()
    {
        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("chromium missing"));

        var manager = new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);

        await Assert.ThrowsAsync<BrowserLaunchException>(
            () => manager.CreateSessionAsync(new SessionOptions()));
        Assert.Empty(await manager.GetAllSessionsAsync());
    }

    private static BrowserManager CreateManager() => CreateManagerWithBrowser().Manager;

    private static (BrowserManager Manager, Mock<IBrowser> Browser, Mock<IPage> Page) CreateManagerWithBrowser()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), It.IsAny<UserAgentMetadata>()))
            .Returns(Task.CompletedTask);
        var browser = new Mock<IBrowser>();
        browser.Setup(b => b.NewPageAsync()).ReturnsAsync(page.Object);
        browser.Setup(b => b.CloseAsync()).Returns(Task.CompletedTask);
        var engine = new Mock<IBrowserEngine>();
        engine.Setup(e => e.LaunchAsync(It.IsAny<LaunchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(browser.Object);
        return (new BrowserManager(engine.Object, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance), browser, page);
    }
}

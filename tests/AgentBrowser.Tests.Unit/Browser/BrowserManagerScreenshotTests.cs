using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;
using AgentBrowserScreenshotOptions = AgentBrowser.Shared.Models.ScreenshotOptions;

namespace AgentBrowser.Tests.Unit.Browser;

public class BrowserManagerScreenshotTests
{
    [Fact]
    public async Task ScreenshotAsync_Viewport_ReturnsBytes()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.ScreenshotDataAsync(It.IsAny<PuppeteerSharp.ScreenshotOptions>()))
            .ReturnsAsync(bytes);

        var session = await manager.CreateSessionAsync(new SessionOptions());
        var result = await manager.ScreenshotAsync(session.Id, new AgentBrowserScreenshotOptions { Type = AgentBrowser.Shared.Models.ScreenshotType.Viewport });

        Assert.Same(bytes, result);
        page.Verify(p => p.ScreenshotDataAsync(It.Is<PuppeteerSharp.ScreenshotOptions>(o =>
            o.Type == PuppeteerSharp.ScreenshotType.Png && !o.FullPage && o.Quality == null)), Times.Once);
    }

    [Fact]
    public async Task ScreenshotAsync_FullPage_SetsFullPage()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.ScreenshotDataAsync(It.IsAny<PuppeteerSharp.ScreenshotOptions>()))
            .ReturnsAsync(bytes);

        var session = await manager.CreateSessionAsync(new SessionOptions());
        await manager.ScreenshotAsync(session.Id, new AgentBrowserScreenshotOptions
        {
            Type = AgentBrowser.Shared.Models.ScreenshotType.FullPage,
            Format = "jpeg",
            Quality = 80
        });

        page.Verify(p => p.ScreenshotDataAsync(It.Is<PuppeteerSharp.ScreenshotOptions>(o =>
            o.Type == PuppeteerSharp.ScreenshotType.Jpeg && o.FullPage && o.Quality == 80)), Times.Once);
    }

    [Fact]
    public async Task ScreenshotAsync_MissingElement_ThrowsSelectorException()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.QuerySelectorAsync(".missing")).ReturnsAsync((IElementHandle?)null);

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<AgentBrowser.Shared.Exceptions.SelectorException>(
            () => manager.ScreenshotAsync(session.Id, new AgentBrowserScreenshotOptions
            {
                Type = AgentBrowser.Shared.Models.ScreenshotType.Element,
                Selector = ".missing"
            }));
    }

    [Fact]
    public async Task ScreenshotAsync_InvalidFormat_ThrowsArgumentException()
    {
        var (manager, _) = CreateManagerWithPage();
        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.ScreenshotAsync(session.Id, new AgentBrowserScreenshotOptions { Format = "bmp" }));
    }

    [Fact]
    public async Task ScreenshotAsync_UnknownSession_ThrowsSessionNotFoundException()
    {
        var (manager, _) = CreateManagerWithPage();

        await Assert.ThrowsAsync<SessionNotFoundException>(
            () => manager.ScreenshotAsync("missing", new AgentBrowserScreenshotOptions()));
    }

    private static (BrowserManager Manager, Mock<IPage> Page) CreateManagerWithPage()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), It.IsAny<UserAgentMetadata>()))
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

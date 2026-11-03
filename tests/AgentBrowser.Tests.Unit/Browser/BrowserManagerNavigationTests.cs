using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;

namespace AgentBrowser.Tests.Unit.Browser;

public class BrowserManagerNavigationTests
{
    [Fact]
    public async Task NavigateAsync_UpdatesTabUrlTitleAndStatus()
    {
        var (manager, page) = CreateManagerWithPage();
        const string url = "data:text/html,<title>Hello</title><h1>Hi</h1>";

        page.Setup(p => p.GoToAsync(url, It.IsAny<NavigationOptions>()))
            .ReturnsAsync((IResponse?)null);
        page.Setup(p => p.GetTitleAsync()).ReturnsAsync("Hello");
        page.SetupGet(p => p.Url).Returns(url);

        var session = await manager.CreateSessionAsync(new SessionOptions());
        var result = await manager.NavigateAsync(session.Id, new NavigateRequest { Url = url });

        Assert.Equal("Hello", result.Title);
        Assert.Equal(url, result.Url);
        Assert.Equal(TabStatus.Loaded, session.Tabs[0].Status);
        Assert.Equal("Hello", session.Tabs[0].Title);
        Assert.Equal(url, session.Tabs[0].Url);
    }

    [Fact]
    public async Task NavigateAsync_WhenPageThrows_WrapsInNavigationException()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.GoToAsync(It.IsAny<string>(), It.IsAny<NavigationOptions>()))
            .ThrowsAsync(new InvalidOperationException("bad url"));

        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<AgentBrowser.Shared.Exceptions.NavigationException>(() =>
            manager.NavigateAsync(session.Id, new NavigateRequest { Url = "https://example.invalid" }));
    }

    [Fact]
    public async Task NavigateAsync_WhenUrlIsNotAbsolute_ThrowsNavigationException()
    {
        var (manager, page) = CreateManagerWithPage();
        var session = await manager.CreateSessionAsync(new SessionOptions());

        await Assert.ThrowsAsync<AgentBrowser.Shared.Exceptions.NavigationException>(() =>
            manager.NavigateAsync(session.Id, new NavigateRequest { Url = "not-a-url" }));
        page.Verify(p => p.GoToAsync(It.IsAny<string>(), It.IsAny<NavigationOptions>()), Times.Never);
    }

    [Fact]
    public async Task GetHtmlAsync_ReturnsPageContent()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.GetContentAsync()).ReturnsAsync("<html>ok</html>");

        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.Equal("<html>ok</html>", await manager.GetHtmlAsync(session.Id));
    }

    [Fact]
    public async Task GetContentTextAsync_ReturnsBodyText()
    {
        var (manager, page) = CreateManagerWithPage();
        page.Setup(p => p.EvaluateExpressionAsync<string>(It.IsAny<string>()))
            .ReturnsAsync("Hello");

        var session = await manager.CreateSessionAsync(new SessionOptions());

        Assert.Equal("Hello", await manager.GetContentTextAsync(session.Id));
    }

    [Fact]
    public async Task GetHtmlAsync_UnknownSession_ThrowsSessionNotFoundException()
    {
        var (manager, _) = CreateManagerWithPage();

        await Assert.ThrowsAsync<SessionNotFoundException>(() => manager.GetHtmlAsync("missing"));
    }

    private static (BrowserManager Manager, Mock<IPage> Page) CreateManagerWithPage()
    {
        var page = new Mock<IPage>();
        page.Setup(p => p.SetViewportAsync(It.IsAny<ViewPortOptions>())).Returns(Task.CompletedTask);
        page.Setup(p => p.SetUserAgentAsync(It.IsAny<string>(), It.IsAny<UserAgentMetadata>()))
            .Returns(Task.CompletedTask);
        page.Setup(p => p.GetTitleAsync()).ReturnsAsync("Untitled");
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

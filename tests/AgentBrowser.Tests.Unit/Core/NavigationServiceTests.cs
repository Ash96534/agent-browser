using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Services;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgentBrowser.Tests.Unit.Core;

public class NavigationServiceTests
{
    [Fact]
    public async Task NavigateAsync_ReturnsMappedResponse()
    {
        var request = new NavigateRequest { Url = "https://example.com" };
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.NavigateAsync("session-1", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PageNavigationResult(request.Url, "Example Domain"));

        var service = new NavigationService(manager.Object, NullLogger<NavigationService>.Instance);
        var response = await service.NavigateAsync("session-1", request);

        Assert.True(response.Success);
        Assert.Equal(request.Url, response.Url);
        Assert.Equal("Example Domain", response.Title);
        Assert.NotEqual(default, response.Timestamp);
    }

    [Fact]
    public async Task GetHtmlAsync_ReturnsHtmlResponse()
    {
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.GetHtmlAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html>ok</html>");

        var service = new NavigationService(manager.Object, NullLogger<NavigationService>.Instance);
        var response = await service.GetHtmlAsync("session-1");

        Assert.Equal("<html>ok</html>", response.Html);
    }

    [Fact]
    public async Task GetContentAsync_ReturnsBodyText()
    {
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.GetContentAsync("session-1", ContentFormat.Text, It.IsAny<CancellationToken>()))
            .ReturnsAsync("body text");

        var service = new NavigationService(manager.Object, NullLogger<NavigationService>.Instance);
        var response = await service.GetContentAsync("session-1");

        Assert.Equal("body text", response.Html);
    }
}

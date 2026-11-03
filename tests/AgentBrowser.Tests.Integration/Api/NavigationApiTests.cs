using System.Net;
using System.Net.Http.Json;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public class NavigationApiTests
{
    [Fact]
    public async Task Navigate_ReturnsNavigationResult()
    {
        using var factory = new ApiFactory();
        var request = new NavigateRequest { Url = "https://example.com" };
        factory.Manager
            .Setup(m => m.NavigateAsync("session-1", It.IsAny<NavigateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PageNavigationResult(request.Url, "Example Domain"));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/session-1/navigate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<NavigateResponse>();
        Assert.True(body!.Success);
        Assert.Equal("Example Domain", body.Title);
    }

    [Fact]
    public async Task Navigate_UnknownSession_ReturnsNotFound()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.NavigateAsync("missing", It.IsAny<NavigateRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SessionNotFoundException("missing"));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/missing/navigate",
            new NavigateRequest { Url = "https://example.com" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Navigate_InvalidUrl_ReturnsBadRequest()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.NavigateAsync("session-1", It.IsAny<NavigateRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NavigationException("invalid url", new ArgumentException()));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session/session-1/navigate",
            new NavigateRequest { Url = "not-a-url" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHtml_ReturnsHtml()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.GetHtmlAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html>ok</html>");

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1/html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HtmlResponse>();
        Assert.Equal("<html>ok</html>", body!.Html);
    }

    [Fact]
    public async Task GetContent_ReturnsBodyText()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.GetContentTextAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("visible text");

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1/content");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HtmlResponse>();
        Assert.Equal("visible text", body!.Html);
    }
}

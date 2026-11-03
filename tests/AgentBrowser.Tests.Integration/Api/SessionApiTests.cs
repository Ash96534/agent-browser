using System.Net;
using System.Net.Http.Json;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Responses;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public class SessionApiTests
{
    [Fact]
    public async Task CreateSession_ReturnsCreatedSession()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.CreateSessionAsync(It.IsAny<SessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSession("session-1"));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session", new SessionOptions());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(body);
        Assert.Equal("session-1", body!.Id);
    }

    [Fact]
    public async Task CreateSession_BrowserLaunchFailure_ReturnsServiceUnavailable()
    {
        using var factory = new ApiFactory();
        factory.Manager
            .Setup(m => m.CreateSessionAsync(It.IsAny<SessionOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BrowserLaunchException("chromium missing", new InvalidOperationException()));

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/session", new SessionOptions());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task GetSession_UnknownId_ReturnsNotFound()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.GetSessionAsync("missing"))
            .ReturnsAsync((Session?)null);

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSession_KnownId_ReturnsSession()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.GetSessionAsync("session-1"))
            .ReturnsAsync(CreateSession("session-1"));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session/session-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.Equal("session-1", body!.Id);
    }

    [Fact]
    public async Task CloseSession_UnknownId_ReturnsNotFound()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.CloseSessionAsync("missing"))
            .ReturnsAsync(false);

        using var client = factory.CreateClient();
        var response = await client.DeleteAsync("/session/missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListSessions_ReturnsSessions()
    {
        using var factory = new ApiFactory();
        factory.Manager.Setup(m => m.GetAllSessionsAsync())
            .ReturnsAsync(new[] { CreateSession("session-1") });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<SessionResponse>>();
        Assert.Single(body!);
    }

    private static Session CreateSession(string id) => new()
    {
        Id = id,
        CreatedAt = DateTime.UtcNow,
        LastAccessedAt = DateTime.UtcNow,
        Status = SessionStatus.Active,
        Tabs =
        [
            new Tab
            {
                Id = "tab-1",
                Url = "about:blank",
                Status = TabStatus.Loaded,
                CreatedAt = DateTime.UtcNow
            }
        ]
    };
}

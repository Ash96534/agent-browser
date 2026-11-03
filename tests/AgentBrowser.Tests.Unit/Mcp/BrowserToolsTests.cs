using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Extraction;
using AgentBrowser.Core.Services;
using AgentBrowser.MCP.Tools;
using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;

namespace AgentBrowser.Tests.Unit.Mcp;

public class BrowserToolsTests
{
    [Fact]
    public async Task CreateSession_CallsSessionService()
    {
        var session = new Mock<ISessionService>();
        SessionOptions? captured = null;
        session.Setup(s => s.CreateSessionAsync(It.IsAny<SessionOptions>()))
            .ReturnsAsync(new SessionResponse { Id = "session-1", CreatedAt = DateTime.UtcNow })
            .Callback<SessionOptions>(options => captured = options);

        using var provider = BuildProvider(services => services.AddSingleton(session.Object));
        var result = await BrowserTools.CreateSession(provider, headless: false);

        Assert.Equal("session-1", result.Id);
        Assert.NotNull(captured);
        Assert.False(captured!.Headless);
    }

    [Fact]
    public async Task CloseSession_ReturnsServiceResult()
    {
        var session = new Mock<ISessionService>();
        session.Setup(s => s.CloseSessionAsync("session-1")).ReturnsAsync(true);

        using var provider = BuildProvider(services => services.AddSingleton(session.Object));
        Assert.True(await BrowserTools.CloseSession(provider, "session-1"));
    }

    [Fact]
    public async Task Navigate_UnknownWaitUntil_ThrowsArgumentException()
    {
        var navigation = new Mock<INavigationService>();
        using var provider = BuildProvider(services => services.AddSingleton(navigation.Object));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            BrowserTools.Navigate(provider, "session-1", "https://example.com", waitUntil: "bogus"));
    }

    [Fact]
    public async Task Click_PassesSelectorAndAggregateOptions()
    {
        var dom = new Mock<IDomService>();
        dom.Setup(d => d.ClickAsync("session-1", It.IsAny<ClickRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var provider = BuildProvider(services => services.AddSingleton(dom.Object));
        Assert.True(await BrowserTools.Click(provider, "session-1", ".btn", delay: 10, button: 2, clickCount: 2));

        dom.Verify(d => d.ClickAsync("session-1",
            It.Is<ClickRequest>(r => r.Selector == ".btn" &&
                r.Options != null && r.Options.Delay == 10 &&
                r.Options.Button == 2 && r.Options.ClickCount == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Navigate_UnknownSession_PropagatesSessionNotFoundException()
    {
        var navigation = new Mock<INavigationService>();
        navigation.Setup(n => n.NavigateAsync("missing", It.IsAny<NavigateRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SessionNotFoundException("missing"));

        using var provider = BuildProvider(services => services.AddSingleton(navigation.Object));

        await Assert.ThrowsAsync<SessionNotFoundException>(() =>
            BrowserTools.Navigate(provider, "missing", "https://example.com"));
    }

    [Fact]
    public async Task GetPrices_UsesExtractionService()
    {
        var manager = new Mock<IBrowserManager>();
        manager.Setup(m => m.EvaluateJsonFunctionAsync(
                "session-1", It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("""[{"value":19.99,"currency":"USD","currencySymbol":"$","context":"product-card","selector":".price"}]""");

        using var provider = BuildProvider(services =>
        {
            services.AddSingleton(manager.Object);
            services.AddSingleton<ExtractionService>();
        });

        var prices = await BrowserTools.GetPrices(provider, "session-1");

        Assert.Single(prices);
        Assert.Equal(19.99m, prices[0].Value);
        Assert.Equal("USD", prices[0].Currency);
    }

    private static ServiceProvider BuildProvider(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        configure(services);
        return services.BuildServiceProvider();
    }
}

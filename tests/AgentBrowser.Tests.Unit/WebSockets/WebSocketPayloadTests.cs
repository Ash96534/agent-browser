using AgentBrowser.Api.WebSockets;
using AgentBrowser.Browser.Browser;

namespace AgentBrowser.Tests.Unit.WebSockets;

public class WebSocketPayloadTests
{
    [Fact]
    public void ConsoleEvent_MapsToTransportMessage()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var message = WebSocketPayload.FromBrowserEvent(
            new BrowserConsoleEvent("session-1", "Log", "hello", timestamp));

        var console = Assert.IsType<ConsoleEvent>(message);
        Assert.Equal("console", console.Type);
        Assert.Equal("session-1", console.SessionId);
        Assert.Equal("Log", console.Level);
        Assert.Equal("hello", console.Message);
        Assert.Equal(timestamp.UtcDateTime, console.Timestamp);
    }

    [Fact]
    public void LoadCompleteEvent_MapsToTransportMessage()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var message = WebSocketPayload.FromBrowserEvent(
            new BrowserLoadCompleteEvent("session-1", "https://example.com", timestamp));

        var load = Assert.IsType<LoadCompleteEvent>(message);
        Assert.Equal("load-complete", load.Type);
        Assert.Equal("https://example.com", load.Url);
        Assert.Equal(timestamp.UtcDateTime, load.LoadTime);
    }

    [Fact]
    public void PageErrorEvent_MapsToTransportMessage()
    {
        var message = WebSocketPayload.FromBrowserEvent(
            new BrowserErrorEvent("session-1", "boom", DateTimeOffset.UtcNow));

        var error = Assert.IsType<ErrorEvent>(message);
        Assert.Equal("error", error.Type);
        Assert.Equal("boom", error.Message);
    }
}

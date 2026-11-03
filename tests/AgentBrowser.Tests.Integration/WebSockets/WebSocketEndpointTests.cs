using System.Net.WebSockets;
using System.Text;
using AgentBrowser.Api.WebSockets;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Tests.Integration.Api;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AgentBrowser.Tests.Integration.WebSockets;

[Trait("Category", "WebSocket")]
public class WebSocketEndpointTests
{
    [Fact]
    public async Task ConsoleEvent_StreamsToMatchingSession()
    {
        using var factory = new ApiFactory();
        var sink = factory.Services.GetRequiredService<IBrowserEventSink>();
        var client = factory.Server.CreateWebSocketClient();
        using var webSocket = await client.ConnectAsync(
            new Uri("ws://localhost/ws/session/session-1"), CancellationToken.None);

        await Task.Delay(100);
        await sink.PublishAsync(new BrowserConsoleEvent(
            "session-1", "Log", "hello", DateTimeOffset.UtcNow));

        var buffer = new byte[4096];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        Assert.Contains("\"type\":\"console\"", json);
        Assert.Contains("\"message\":\"hello\"", json);
        Assert.Contains("\"sessionId\":\"session-1\"", json);

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }
}

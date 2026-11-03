using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentBrowser.Tests.Integration.Extraction;

internal static class BrowserExtractionHarness
{
    public static async Task<BrowserManager> CreateNavigatedManagerAsync(string html)
    {
        var engine = new PuppeteerBrowserEngine();
        var manager = new BrowserManager(engine, NullBrowserEventSink.Instance,
            NullLogger<BrowserManager>.Instance);

        try
        {
            var session = await manager.CreateSessionAsync(new SessionOptions());
            await manager.NavigateAsync(session.Id, new NavigateRequest
            {
                Url = ToDataUrl(html),
                WaitUntil = WaitUntil.Load
            });
            return manager;
        }
        catch
        {
            await manager.DisposeAsync();
            throw;
        }
    }

    public static string ToDataUrl(string html) =>
        "data:text/html," + Uri.EscapeDataString(html);
}

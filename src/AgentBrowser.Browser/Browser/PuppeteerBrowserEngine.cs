using AgentBrowser.Shared.Exceptions;
using PuppeteerSharp;
using System.IO;
using System.Linq;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Launches Chromium through PuppeteerSharp, installing it once on first use.
/// </summary>
public sealed class PuppeteerBrowserEngine : IBrowserEngine
{
    private static readonly SemaphoreSlim DownloadGate = new(1, 1);
    private static readonly string BrowserCachePath =
        Environment.GetEnvironmentVariable("AGENTBROWSER_BROWSER_CACHE")
        ?? Path.Combine(AppContext.BaseDirectory, ".agentbrowser-cache");

    /// <inheritdoc />
    public async Task<IBrowser> LaunchAsync(LaunchOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var fetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = BrowserCachePath
        });

        await EnsureBrowserInstalledAsync(fetcher, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(options.ExecutablePath))
        {
            options.ExecutablePath = GetInstalledChromePath(fetcher);
        }

        return await Puppeteer.LaunchAsync(options).ConfigureAwait(false);
    }

    private static string GetInstalledChromePath(BrowserFetcher fetcher)
    {
        var installed = fetcher.GetInstalledBrowsers()
            .Where(browser => browser.Browser == SupportedBrowser.Chrome)
            .OrderByDescending(browser => browser.BuildId, StringComparer.Ordinal)
            .FirstOrDefault();

        if (installed is null)
        {
            throw new BrowserLaunchException(
                "Chromium is not installed. Delete the browser cache and retry browse_create_session.",
                new InvalidOperationException("No installed Chrome browser was found in the PuppeteerSharp cache."));
        }

        return installed.GetExecutablePath();
    }

    private static async Task EnsureBrowserInstalledAsync(BrowserFetcher fetcher, CancellationToken cancellationToken)
    {
        await DownloadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await fetcher.DownloadAsync().ConfigureAwait(false);
        }
        finally
        {
            DownloadGate.Release();
        }
    }
}

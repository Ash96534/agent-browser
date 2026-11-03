using PuppeteerSharp;
using System.IO;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Launches Chromium through PuppeteerSharp, installing it once on first use.
/// </summary>
public sealed class PuppeteerBrowserEngine : IBrowserEngine
{
    private static readonly SemaphoreSlim DownloadGate = new(1, 1);
    private static readonly string BrowserCachePath =
        Environment.GetEnvironmentVariable("AGENTBROWSER_BROWSER_CACHE")
        ?? Path.Combine("D:\\kiro_proj", ".agentbrowser-cache");

    /// <inheritdoc />
    public async Task<IBrowser> LaunchAsync(LaunchOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await EnsureBrowserInstalledAsync(cancellationToken).ConfigureAwait(false);
        return await Puppeteer.LaunchAsync(options).ConfigureAwait(false);
    }

    private static async Task EnsureBrowserInstalledAsync(CancellationToken cancellationToken)
    {
        await DownloadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var fetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = BrowserCachePath
            });
            await fetcher.DownloadAsync().ConfigureAwait(false);
        }
        finally
        {
            DownloadGate.Release();
        }
    }
}

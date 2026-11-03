using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Launches Chromium through PuppeteerSharp, installing it once on first use.
/// </summary>
public sealed class PuppeteerBrowserEngine : IBrowserEngine
{
    private readonly SemaphoreSlim _installGate = new(1, 1);
    private bool _installed;

    /// <inheritdoc />
    public async Task<IBrowser> LaunchAsync(LaunchOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!_installed)
        {
            await EnsureBrowserInstalledAsync(cancellationToken).ConfigureAwait(false);
        }

        return await Puppeteer.LaunchAsync(options).ConfigureAwait(false);
    }

    private async Task EnsureBrowserInstalledAsync(CancellationToken cancellationToken)
    {
        if (_installed)
        {
            return;
        }

        await _installGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_installed)
            {
                return;
            }

            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync().ConfigureAwait(false);
            _installed = true;
        }
        finally
        {
            _installGate.Release();
        }
    }
}

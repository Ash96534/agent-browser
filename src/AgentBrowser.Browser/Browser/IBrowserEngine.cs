using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Thin seam over Chromium launch so session lifecycle can be unit-tested.
/// </summary>
public interface IBrowserEngine
{
    /// <summary>
    /// Launches a browser instance using the supplied launch options.
    /// </summary>
    Task<IBrowser> LaunchAsync(LaunchOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the path of the installed Chromium executable, or null when none is available.
    /// </summary>
    Task<string?> ResolveChromiumExecutablePathAsync(CancellationToken cancellationToken = default);
}

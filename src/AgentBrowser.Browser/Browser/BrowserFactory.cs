using AgentBrowser.Shared.Models;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Browser;

public class BrowserFactory
{
    private static bool _isChromiumInstalled;

    public async Task<IBrowser> CreateBrowserAsync(SessionOptions options)
    {
        if (!_isChromiumInstalled)
        {
            await InstallChromiumAsync();
            _isChromiumInstalled = true;
        }
        
        var launchOptions = GetLaunchOptions(options);
        
        return await Puppeteer.LaunchAsync(launchOptions);
    }

    private static async Task InstallChromiumAsync()
    {
        _ = await BrowserFetcher.DownloadAsync();
    }

    private LaunchOptions GetLaunchOptions(SessionOptions options)
    {
        return new LaunchOptions
        {
            Headless = options.Headless ? HeadlessModes.New : HeadlessModes.No,
            Args = GetArgs(options),
            IgnoreHTTPSErrors = options.IgnoreHTTPSErrors,
            DumpIO = options.DumpIO,
            DefaultViewport = options.Viewport != null ? 
                new ViewPortOptions
                {
                    Width = options.Viewport.Width,
                    Height = options.Viewport.Height,
                    DeviceScaleFactor = options.Viewport.DeviceScaleFactor,
                    IsMobile = options.Viewport.IsMobile,
                    HasTouch = options.Viewport.HasTouch
                } : null
        };
    }

    private List<string> GetArgs(SessionOptions options)
    {
        var args = new List<string>
        {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-web-security",
            "--disable-features=IsolateOrigins,site-per-process",
            "--disable-features=AutoExpandToFullURL"
        };

        if (options.Args != null)
        {
            args.AddRange(options.Args);
        }

        return args;
    }
}
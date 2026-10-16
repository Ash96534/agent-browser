using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentBrowserCore(
        this IServiceCollection services,
        Action<BrowserSettings>? configureBrowser = null)
    {
        services.AddSingleton<IBrowserManager, BrowserManager>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IDomService, DomService>();
        services.AddScoped<IScreenshotService, ScreenshotService>();
        
        if (configureBrowser != null)
        {
            services.Configure(configureBrowser);
        }
        
        return services;
    }
}

public class BrowserSettings
{
    public bool Headless { get; set; } = true;
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
}
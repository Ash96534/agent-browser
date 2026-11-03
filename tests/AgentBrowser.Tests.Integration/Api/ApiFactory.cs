using AgentBrowser.Browser.Browser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace AgentBrowser.Tests.Integration.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public Mock<IBrowserManager> Manager { get; } = new(MockBehavior.Loose);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBrowserManager>();
            services.AddSingleton(Manager.Object);
        });
    }
}

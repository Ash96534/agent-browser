using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Extraction;
using AgentBrowser.Core.Services;
using AgentBrowser.MCP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddAgentBrowserCore();
});

var host = builder.Build();

// Initialize service provider
var serviceProvider = host.Services;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<McpServer>();

var server = new McpServer(serviceProvider, logger);

Console.WriteLine("AgentBrowser MCP Server starting...");
Console.WriteLine("Use stdin/stdout for MCP protocol communication.");

await server.StartAsync();

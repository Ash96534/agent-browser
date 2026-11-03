using AgentBrowser.Api.Middleware;
using AgentBrowser.Api.WebSockets;
using AgentBrowser.Browser.Browser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAgentBrowserCore();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IBrowserEventSink>(sp => sp.GetRequiredService<WebSocketHandler>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();
app.MapControllers();

app.Run();

public partial class Program { }

using AgentBrowser.Api.WebSockets;
using AgentBrowser.Browser.Browser;
using AgentBrowser.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register AgentBrowser services
builder.Services.AddAgentBrowserCore();

// Register WebSocket handler
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Enable WebSocket middleware
app.UseWebSockets();

app.MapControllers();

app.Run();

using AgentBrowser.Api.Middleware;
using AgentBrowser.Api.Persistence;
using AgentBrowser.Api.WebSockets;
using AgentBrowser.Browser.Browser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAgentBrowserCore();
builder.Services.AddSingleton<OperationStore>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IBrowserEventSink>(sp => sp.GetRequiredService<WebSocketHandler>());

var corsOrigins = Environment.GetEnvironmentVariable("AGENTBROWSER_CORS_ORIGINS");
var allowedCorsOrigins = corsOrigins?.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

if (allowedCorsOrigins.Length > 0)
{
    builder.Services.AddCors();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiVersionMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestSizeLimitMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseMiddleware<OperationTimingMiddleware>();

if (allowedCorsOrigins.Length > 0)
{
    app.UseCors(policy => policy.WithOrigins(allowedCorsOrigins).AllowAnyHeader().AllowAnyMethod());
}

app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();
app.MapControllers();
app.MapGet("/docs", () => Results.Content("""
    <!doctype html>
    <html>
      <head><title>AgentBrowser API Documentation</title></head>
      <body>
        <h1>AgentBrowser API</h1>
        <ul>
          <li><a href="/capabilities">Capabilities</a></li>
          <li><a href="/diagnostics">Diagnostics</a></li>
          <li><a href="/health">Health</a></li>
        </ul>
        <p>See README.md and SPEC.md in the repository for request and response schemas.</p>
      </body>
    </html>
    """, "text/html; charset=utf-8"));

app.Run();

public partial class Program { }

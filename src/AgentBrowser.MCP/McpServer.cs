using AgentBrowser.Core.Extraction;
using AgentBrowser.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AgentBrowser.MCP;

/// <summary>
/// MCP (Model Context Protocol) Server implementation
/// </summary>
public class McpServer : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpServer> _logger;
    private bool _disposed;

    public McpServer(IServiceProvider serviceProvider, ILogger<McpServer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Start the MCP server using stdio transport
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting MCP server with stdio transport...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var line = await ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line))
                    continue;

                var message = JsonSerializer.Deserialize<McpRequest>(line);
                if (message == null)
                {
                    await SendErrorResponseAsync(-1, "Invalid JSON", cancellationToken);
                    continue;
                }

                var response = await HandleRequestAsync(message, cancellationToken);
                await SendResponseAsync(response, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse MCP request");
                await SendErrorResponseAsync(-1, "Invalid JSON", cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MCP server shutdown requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MCP request");
                await SendErrorResponseAsync(-1, ex.Message, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Read a line from stdin
    /// </summary>
    private async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
        var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        return await reader.ReadLineAsync(cancellationToken);
    }

    /// <summary>
    /// Send response to stdout
    /// </summary>
    private async Task SendResponseAsync(McpResponse response, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var output = Console.OpenStandardOutput();
        var writer = new StreamWriter(output, Encoding.UTF8);
        await writer.WriteLineAsync(json);
        await writer.FlushAsync();
    }

    /// <summary>
    /// Send error response
    /// </summary>
    private async Task SendErrorResponseAsync(int id, string message, CancellationToken cancellationToken)
    {
        var response = new McpResponse
        {
            JsonRpc = "2.0",
            Id = id,
            Error = new McpError
            {
                Code = -32603,
                Message = message
            }
        };

        await SendResponseAsync(response, cancellationToken);
    }

    /// <summary>
    /// Handle MCP request
    /// </summary>
    private async Task<McpResponse> HandleRequestAsync(McpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionService = _serviceProvider.GetRequiredService<ISessionService>();

            switch (request.Method)
            {
                case "browse/navigate":
                    return await HandleBrowseNavigateAsync(request, sessionService, cancellationToken);

                case "browse/click":
                    return await HandleBrowseClickAsync(request, sessionService, cancellationToken);

                case "browse/type":
                    return await HandleBrowseTypeAsync(request, sessionService, cancellationToken);

                case "browse/getHtml":
                    return await HandleBrowseGetHtmlAsync(request, sessionService, cancellationToken);

                case "browse/getScreenshot":
                    return await HandleBrowseGetScreenshotAsync(request, sessionService, cancellationToken);

                case "browse/executeJs":
                    return await HandleBrowseExecuteJsAsync(request, sessionService, cancellationToken);

                case "browse/extractSchema":
                    return await HandleBrowseExtractSchemaAsync(request, sessionService, cancellationToken);

                case "browse/getMetadata":
                    return await HandleBrowseGetMetadataAsync(request, sessionService, cancellationToken);

                case "browse/getPrices":
                    return await HandleBrowseGetPricesAsync(request, sessionService, cancellationToken);

                case "browse/getImages":
                    return await HandleBrowseGetImagesAsync(request, sessionService, cancellationToken);

                case "browse/getForms":
                    return await HandleBrowseGetFormsAsync(request, sessionService, cancellationToken);

                default:
                    return new McpResponse
                    {
                        JsonRpc = "2.0",
                        Id = request.Id,
                        Error = new McpError
                        {
                            Code = -32601,
                            Message = $"Method not found: {request.Method}"
                        }
                    };
            }
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                JsonRpc = "2.0",
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = ex.Message
                }
            };
        }
    }

    private async Task<McpResponse> HandleBrowseNavigateAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var url = request.Params["url"]?.ToString() ?? throw new ArgumentNullException("url");

        var session = await sessionService.CreateSessionAsync(new AgentBrowser.Shared.Models.SessionOptions { Headless = true });

        var navService = _serviceProvider.GetRequiredService<INavigationService>();
        var navigateRequest = new AgentBrowser.Shared.Models.Requests.NavigateRequest { Url = url };
        var result = await navService.NavigateAsync(sessionId, sessionId, navigateRequest);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { success = result.Success, url = result.Url }
        };
    }

    private async Task<McpResponse> HandleBrowseClickAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();

        var domService = _serviceProvider.GetRequiredService<IDomService>();
        var selector = request.Params["selector"]?.ToString() ?? throw new ArgumentNullException("selector");
        await domService.ClickAsync(sessionId, sessionId, new AgentBrowser.Shared.Models.Requests.ClickRequest { Selector = selector });

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { success = true, selector = selector }
        };
    }

    private async Task<McpResponse> HandleBrowseTypeAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();

        var domService = _serviceProvider.GetRequiredService<IDomService>();
        var selector = request.Params["selector"]?.ToString() ?? throw new ArgumentNullException("selector");
        var text = request.Params["text"]?.ToString() ?? throw new ArgumentNullException("text");

        await domService.TypeAsync(sessionId, sessionId, new AgentBrowser.Shared.Models.Requests.TypeRequest
        {
            Selector = selector,
            Text = text
        });

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { success = true, selector = selector }
        };
    }

    private async Task<McpResponse> HandleBrowseGetHtmlAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();

        var navService = _serviceProvider.GetRequiredService<INavigationService>();
        var result = await navService.GetHtmlAsync(sessionId, sessionId);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { html = result.Html }
        };
    }

    private async Task<McpResponse> HandleBrowseGetScreenshotAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();

        var screenshotService = _serviceProvider.GetRequiredService<IScreenshotService>();
        var result = await screenshotService.CaptureBase64Async(sessionId, sessionId, new AgentBrowser.Shared.Models.Requests.ScreenshotRequest());

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { base64 = result }
        };
    }

    private async Task<McpResponse> HandleBrowseExecuteJsAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();

        var domService = _serviceProvider.GetRequiredService<IDomService>();
        var script = request.Params["script"]?.ToString() ?? throw new ArgumentNullException("script");

        var result = await domService.ExecuteJsAsync<object>(sessionId, sessionId, new AgentBrowser.Shared.Models.Requests.ExecuteJsRequest { Script = script });

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { result = result }
        };
    }

    private async Task<McpResponse> HandleBrowseExtractSchemaAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var extractionService = _serviceProvider.GetRequiredService<ExtractionService>();

        var schema = request.Params["schema"]?.ToString() ?? "metadata";
        object result;

        switch (schema.ToLower())
        {
            case "metadata":
                result = await extractionService.ExtractMetadataAsync(sessionId, sessionId);
                break;
            case "prices":
                result = await extractionService.ExtractPricesAsync(sessionId, sessionId);
                break;
            case "images":
                result = await extractionService.ExtractImagesAsync(sessionId, sessionId);
                break;
            case "forms":
                result = await extractionService.ExtractFormsAsync(sessionId, sessionId);
                break;
            case "links":
                result = await extractionService.ExtractLinksAsync(sessionId, sessionId);
                break;
            default:
                result = await extractionService.ExtractMetadataAsync(sessionId, sessionId);
                break;
        }

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { extracted = result }
        };
    }

    private async Task<McpResponse> HandleBrowseGetMetadataAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var extractionService = _serviceProvider.GetRequiredService<ExtractionService>();
        var result = await extractionService.ExtractMetadataAsync(sessionId, sessionId);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { metadata = result }
        };
    }

    private async Task<McpResponse> HandleBrowseGetPricesAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var extractionService = _serviceProvider.GetRequiredService<ExtractionService>();
        var result = await extractionService.ExtractPricesAsync(sessionId, sessionId);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { prices = result }
        };
    }

    private async Task<McpResponse> HandleBrowseGetImagesAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var extractionService = _serviceProvider.GetRequiredService<ExtractionService>();
        var result = await extractionService.ExtractImagesAsync(sessionId, sessionId);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { images = result }
        };
    }

    private async Task<McpResponse> HandleBrowseGetFormsAsync(McpRequest request, ISessionService sessionService, CancellationToken cancellationToken)
    {
        var sessionId = request.Params["sessionId"]?.ToString() ?? Guid.NewGuid().ToString();
        var extractionService = _serviceProvider.GetRequiredService<ExtractionService>();
        var result = await extractionService.ExtractFormsAsync(sessionId, sessionId);

        return new McpResponse
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { forms = result }
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}

/// <summary>
/// MCP Request message
/// </summary>
public class McpRequest
{
    public string JsonRpc { get; set; } = "2.0";
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, object> Params { get; set; } = new();
}

/// <summary>
/// MCP Response message
/// </summary>
public class McpResponse
{
    public string JsonRpc { get; set; } = "2.0";
    public int? Id { get; set; }
    public object? Result { get; set; }
    public McpError? Error { get; set; }
}

/// <summary>
/// MCP Error object
/// </summary>
public class McpError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
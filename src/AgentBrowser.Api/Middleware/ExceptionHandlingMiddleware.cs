using AgentBrowser.Shared.Exceptions;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Api.Middleware;

/// <summary>
/// Converts known exceptions into consistent HTTP error responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Executes the pipeline and maps exceptions.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, message) = MapException(ex);
            if (statusCode >= 500)
            {
                _logger.LogError(ex, "Request failed with status {StatusCode}", statusCode);
            }
            else
            {
                _logger.LogWarning(ex, "Request rejected with status {StatusCode}", statusCode);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Error = message,
                Message = message,
                StackTrace = _environment.IsDevelopment() ? ex.ToString() : null
            });
        }
    }

    private static (int StatusCode, string Message) MapException(Exception ex) => ex switch
    {
        SessionNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
        KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
        BrowserLaunchException => (StatusCodes.Status503ServiceUnavailable, ex.Message),
        NavigationException => (StatusCodes.Status400BadRequest, ex.Message),
        SelectorException => (StatusCodes.Status400BadRequest, ex.Message),
        ScriptExecutionException => (StatusCodes.Status400BadRequest, ex.Message),
        ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}

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

            var (statusCode, kind, message, retryable) = MapException(ex);
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
                Kind = kind.ToString(),
                Error = message,
                Message = message,
                TraceId = context.Items[AgentBrowser.Shared.Models.ApiContract.CorrelationIdHeader] as string,
                Retryable = retryable,
                StackTrace = _environment.IsDevelopment() ? ex.ToString() : null
            });
        }
    }

    private static (int StatusCode, ErrorKind Kind, string Message, bool Retryable) MapException(Exception ex) => ex switch
    {
        SessionNotFoundException => (StatusCodes.Status404NotFound, ErrorKind.SessionNotFound, ex.Message, false),
        KeyNotFoundException => (StatusCodes.Status404NotFound, ErrorKind.SessionNotFound, ex.Message, false),
        RequestTooLargeException => (StatusCodes.Status413PayloadTooLarge, ErrorKind.RequestTooLarge, ex.Message, false),
        BrowserLaunchException => (StatusCodes.Status503ServiceUnavailable, ErrorKind.BrowserLaunchFailure, ex.Message, true),
        NavigationException => (StatusCodes.Status400BadRequest, ErrorKind.NavigationFailure, ex.Message, false),
        SelectorException => (StatusCodes.Status400BadRequest, ErrorKind.SelectorNotFound, ex.Message, false),
        ScriptExecutionException => (StatusCodes.Status400BadRequest, ErrorKind.ScriptFailure, ex.Message, false),
        ArgumentException => (StatusCodes.Status400BadRequest, ErrorKind.InvalidRequest, ex.Message, false),
        TimeoutException => (StatusCodes.Status408RequestTimeout, ErrorKind.Timeout, ex.Message, true),
        _ => (StatusCodes.Status500InternalServerError, ErrorKind.Unknown, "An unexpected error occurred.", false)
    };
}

using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Requests;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

/// <summary>
/// Thin adapter over the browser DOM operations.
/// </summary>
public sealed class DomService : IDomService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<DomService> _logger;

    /// <summary>
    /// Creates a DOM service.
    /// </summary>
    public DomService(IBrowserManager browserManager, ILogger<DomService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ClickAsync(string sessionId, ClickRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _browserManager
            .ClickAsync(sessionId, request.Selector, request.Options ?? new ClickOptions(), cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Clicked element with selector {Selector}", request.Selector);
    }

    /// <inheritdoc />
    public async Task TypeAsync(string sessionId, TypeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _browserManager
            .TypeAsync(sessionId, request.Selector, request.Text, request.Delay, request.ClearFirst, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Typed text into element with selector {Selector}", request.Selector);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteJsAsync<T>(string sessionId, ExecuteJsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _browserManager
            .EvaluateExpressionAsync<T>(sessionId, request.Script, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Executed JavaScript of length {ScriptLength}", request.Script.Length);
        return result;
    }

    /// <inheritdoc />
    public async Task WaitForSelectorAsync(string sessionId, string selector, int timeout = 30000, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(selector);

        await _browserManager
            .WaitForSelectorAsync(sessionId, selector, timeout, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Waited for selector {Selector}", selector);
    }
}

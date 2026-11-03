namespace AgentBrowser.Browser.Browser;

/// <summary>
/// Receives browser events for forwarding to observers.
/// </summary>
public interface IBrowserEventSink
{
    /// <summary>
    /// Publishes a browser event.
    /// </summary>
    ValueTask PublishAsync(BrowserEvent browserEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sink used when no observer is registered.
/// </summary>
public sealed class NullBrowserEventSink : IBrowserEventSink
{
    /// <summary>
    /// Shared no-op sink instance.
    /// </summary>
    public static NullBrowserEventSink Instance { get; } = new();

    private NullBrowserEventSink()
    {
    }

    /// <inheritdoc />
    public ValueTask PublishAsync(BrowserEvent browserEvent, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

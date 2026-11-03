namespace AgentBrowser.Shared.Models;

/// <summary>
/// A console message emitted by a page.
/// </summary>
public sealed record ConsoleLogEntry(string Level, string Message, DateTimeOffset Timestamp);

/// <summary>
/// A page error message.
/// </summary>
public sealed record PageErrorEntry(string Message, DateTimeOffset Timestamp);

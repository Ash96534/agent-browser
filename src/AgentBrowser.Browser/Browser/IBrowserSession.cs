using AgentBrowser.Shared.Models;

namespace AgentBrowser.Browser.Browser;

public interface IBrowserSession : IDisposable
{
    string Id { get; }
    DateTime CreatedAt { get; }
    DateTime LastAccessedAt { get; set; }
    SessionStatus Status { get; }
    IReadOnlyList<IBrowserTab> Tabs { get; }
    
    Task<IBrowserTab> CreateTabAsync();
    Task<IBrowserTab?> GetTabAsync(string tabId);
    Task CloseTabAsync(string tabId);
    Task CloseAsync();
}
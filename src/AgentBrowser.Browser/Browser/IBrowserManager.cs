using AgentBrowser.Shared.Models;

namespace AgentBrowser.Browser.Browser;

public interface IBrowserManager
{
    Task<IBrowserSession> CreateSessionAsync(SessionOptions options);
    Task<IBrowserSession?> GetSessionAsync(string sessionId);
    Task CloseSessionAsync(string sessionId);
    Task<IEnumerable<IBrowserSession>> GetAllSessionsAsync();
    Task CleanupIdleSessionsAsync(TimeSpan maxIdleTime);
}
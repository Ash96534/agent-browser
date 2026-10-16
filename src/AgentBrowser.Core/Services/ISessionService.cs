using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Responses;

namespace AgentBrowser.Core.Services;

public interface ISessionService
{
    Task<SessionResponse> CreateSessionAsync(AgentBrowser.Shared.Models.SessionOptions options);
    Task<SessionResponse?> GetSessionAsync(string sessionId);
    Task<bool> CloseSessionAsync(string sessionId);
    Task<IEnumerable<SessionResponse>> GetAllSessionsAsync();
}
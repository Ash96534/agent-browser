using AgentBrowser.Browser.Browser;
using AgentBrowser.Shared.Models;
using AgentBrowser.Shared.Models.Responses;
using Microsoft.Extensions.Logging;

namespace AgentBrowser.Core.Services;

public class SessionService : ISessionService
{
    private readonly IBrowserManager _browserManager;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IBrowserManager browserManager, ILogger<SessionService> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<SessionResponse> CreateSessionAsync(AgentBrowser.Shared.Models.SessionOptions options)
    {
        var session = await _browserManager.CreateSessionAsync(options);
        return new SessionResponse
        {
            Id = session.Id,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt,
            Tabs = session.Tabs.Select(t => new TabResponse
            {
                Id = t.Id,
                Url = t.Url,
                Title = t.Title
            }).ToList()
        };
    }

    public async Task<SessionResponse?> GetSessionAsync(string sessionId)
    {
        var session = await _browserManager.GetSessionAsync(sessionId);
        if (session == null) return null;
        
        return new SessionResponse
        {
            Id = session.Id,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt,
            Tabs = session.Tabs.Select(t => new TabResponse
            {
                Id = t.Id,
                Url = t.Url,
                Title = t.Title
            }).ToList()
        };
    }

    public async Task<bool> CloseSessionAsync(string sessionId)
    {
        try
        {
            await _browserManager.CloseSessionAsync(sessionId);
            _logger.LogInformation("Closed session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<IEnumerable<SessionResponse>> GetAllSessionsAsync()
    {
        var sessions = await _browserManager.GetAllSessionsAsync();
        return sessions.Select(s => new SessionResponse
        {
            Id = s.Id,
            Status = s.Status.ToString(),
            CreatedAt = s.CreatedAt,
            Tabs = s.Tabs.Select(t => new TabResponse
            {
                Id = t.Id,
                Url = t.Url,
                Title = t.Title
            }).ToList()
        });
    }
}

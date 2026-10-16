using AgentBrowser.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AgentBrowser.Browser.Browser;

public class BrowserManager : IBrowserManager
{
    private readonly ConcurrentDictionary<string, BrowserSession> _sessions = new();
    private readonly BrowserFactory _browserFactory;
    private readonly ILogger<BrowserManager> _logger;
    private readonly object _lock = new();

    public BrowserManager(BrowserFactory browserFactory, ILogger<BrowserManager> logger)
    {
        _browserFactory = browserFactory;
        _logger = logger;
    }

    public async Task<IBrowserSession> CreateSessionAsync(SessionOptions options)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new BrowserSession(sessionId, _browserFactory, options);
        
        _sessions[sessionId] = session;
        _logger.LogInformation("Created new browser session {SessionId}", sessionId);
        
        return session;
    }

    public async Task<IBrowserSession?> GetSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastAccessedAt = DateTime.UtcNow;
            return session;
        }
        
        _logger.LogWarning("Session {SessionId} not found", sessionId);
        return null;
    }

    public async Task CloseSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            await session.CloseAsync();
            _logger.LogInformation("Closed browser session {SessionId}", sessionId);
        }
    }

    public async Task<IEnumerable<IBrowserSession>> GetAllSessionsAsync()
    {
        return _sessions.Values.Cast<IBrowserSession>();
    }

    public async Task CleanupIdleSessionsAsync(TimeSpan maxIdleTime)
    {
        var now = DateTime.UtcNow;
        var sessionsToClose = _sessions
            .Where(kv => (now - kv.Value.LastAccessedAt) > maxIdleTime)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var sessionId in sessionsToClose)
        {
            try
            {
                await CloseSessionAsync(sessionId);
                _logger.LogInformation("Cleaned up idle session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up session {SessionId}", sessionId);
            }
        }
    }
}
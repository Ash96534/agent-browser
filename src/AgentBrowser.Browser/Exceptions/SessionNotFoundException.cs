using AgentBrowser.Browser.Exceptions;

namespace AgentBrowser.Browser.Exceptions;

public class SessionNotFoundException : BrowserException
{
    public string SessionId { get; }
    
    public SessionNotFoundException(string sessionId) 
        : base($"Session '{sessionId}' not found")
    {
        SessionId = sessionId;
    }
}
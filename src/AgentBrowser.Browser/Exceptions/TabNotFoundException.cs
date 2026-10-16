using AgentBrowser.Browser.Exceptions;

namespace AgentBrowser.Browser.Exceptions;

public class TabNotFoundException : BrowserException
{
    public string TabId { get; }
    
    public TabNotFoundException(string tabId) 
        : base($"Tab '{tabId}' not found")
    {
        TabId = tabId;
    }
}
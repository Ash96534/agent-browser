using AgentBrowser.Browser.Exceptions;

namespace AgentBrowser.Browser.Exceptions;

public class NavigationException : BrowserException
{
    public string Url { get; }
    
    public NavigationException(string message) : base(message) { }
    public NavigationException(string message, Exception inner) : base(message, inner) { }
    public NavigationException(string url, string message) 
        : base($"Navigation to '{url}' failed: {message}")
    {
        Url = url;
    }
}
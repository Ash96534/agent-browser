using AgentBrowser.Browser.Exceptions;

namespace AgentBrowser.Browser.Exceptions;

public class SelectorException : BrowserException
{
    public string Selector { get; }
    
    public SelectorException(string message) : base(message) { }
    public SelectorException(string message, Exception inner) : base(message, inner) { }
    public SelectorException(string selector, string message) 
        : base($"Selector '{selector}' failed: {message}")
    {
        Selector = selector;
    }
}
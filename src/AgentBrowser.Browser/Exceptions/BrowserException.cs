namespace AgentBrowser.Browser.Exceptions;

public class BrowserException : Exception
{
    public BrowserException() { }
    public BrowserException(string message) : base(message) { }
    public BrowserException(string message, Exception inner) : base(message, inner) { }
}
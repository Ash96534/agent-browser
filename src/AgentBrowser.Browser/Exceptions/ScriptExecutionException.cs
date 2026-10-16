using AgentBrowser.Browser.Exceptions;

namespace AgentBrowser.Browser.Exceptions;

public class ScriptExecutionException : BrowserException
{
    public string Script { get; }
    
    public ScriptExecutionException(string message) : base(message) { }
    public ScriptExecutionException(string message, Exception inner) : base(message, inner) { }
    public ScriptExecutionException(string script, string message) 
        : base($"Script execution failed: {message}")
    {
        Script = script;
    }
}
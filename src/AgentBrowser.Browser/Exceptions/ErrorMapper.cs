using AgentBrowser.Browser.Exceptions;
using PuppeteerSharp;

namespace AgentBrowser.Browser.Exceptions;

public static class ErrorMapper
{
    public static BrowserException MapException(Exception ex)
    {
        return ex switch
        {
            TargetClosedException => new SessionNotFoundException("Target closed"),
            NavigationException navEx => new NavigationException(navEx.Message, navEx),
            SelectorException selEx => new SelectorException(selEx.Message, selEx),
            EvaluationFailedException evalEx => new ScriptExecutionException(evalEx.Message, evalEx),
            _ => new BrowserException(ex.Message, ex)
        };
    }
}
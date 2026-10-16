namespace AgentBrowser.Shared.Models.Requests;

public class ExecuteJsRequest
{
    public string Script { get; set; } = string.Empty;
    public object[]? Args { get; set; }
}
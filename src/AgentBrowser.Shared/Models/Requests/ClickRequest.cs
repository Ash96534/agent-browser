namespace AgentBrowser.Shared.Models.Requests;

public class ClickRequest
{
    public string Selector { get; set; } = string.Empty;
    public ClickOptions? Options { get; set; }
}
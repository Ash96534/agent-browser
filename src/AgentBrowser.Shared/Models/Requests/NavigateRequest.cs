namespace AgentBrowser.Shared.Models.Requests;

public class NavigateRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public int? Timeout { get; set; } = 30000;
    public AgentBrowser.Shared.Models.WaitUntil? WaitUntil { get; set; }
}
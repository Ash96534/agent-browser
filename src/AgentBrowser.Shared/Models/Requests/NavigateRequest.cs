namespace AgentBrowser.Shared.Models.Requests;

public enum WaitUntil
{
    Load,
    DOMContentLoaded,
    NetworkIdle0,
    NetworkIdle2
}

public class NavigateRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public int? Timeout { get; set; } = 30000;
    public WaitUntil? WaitUntil { get; set; }
}
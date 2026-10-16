namespace AgentBrowser.Shared.Models.Requests;

public class NavigateRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public int? Timeout { get; set; } = 30000;
    public WaitUntil? WaitUntil { get; set; } = WaitUntil.Load;
}

public enum WaitUntil
{
    Load,
    DOMContentLoaded,
    NetworkIdle0,
    NetworkIdle2
}
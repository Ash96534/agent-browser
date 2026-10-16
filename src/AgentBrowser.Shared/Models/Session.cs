namespace AgentBrowser.Shared.Models;

public class Session
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public SessionStatus Status { get; set; }
    public List<Tab> Tabs { get; set; } = new();
    public Viewport Viewport { get; set; } = new();
    public string UserAgent { get; set; } = string.Empty;
    public List<Cookie> Cookies { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum SessionStatus
{
    Active,
    Idle,
    Closed
}
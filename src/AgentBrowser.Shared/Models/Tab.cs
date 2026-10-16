namespace AgentBrowser.Shared.Models;

public class Tab
{
    public string Id { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Title { get; set; }
    public TabStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
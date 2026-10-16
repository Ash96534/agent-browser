namespace AgentBrowser.Shared.Models.Responses;

public class NavigateResponse
{
    public bool Success { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
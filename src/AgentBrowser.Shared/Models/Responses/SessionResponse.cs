namespace AgentBrowser.Shared.Models.Responses;

public class SessionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TabResponse> Tabs { get; set; } = new();
}

public class TabResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Title { get; set; }
}
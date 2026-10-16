namespace AgentBrowser.Shared.Models.Requests;

public class ClickRequest
{
    public string Selector { get; set; } = string.Empty;
    public ClickOptions? Options { get; set; }
}

public class ClickOptions
{
    public int Delay { get; set; } = 0;
    public int Button { get; set; } = 0; // 0=left, 1=middle, 2=right
    public int ClickCount { get; set; } = 1;
}
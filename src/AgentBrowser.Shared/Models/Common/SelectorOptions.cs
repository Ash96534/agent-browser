namespace AgentBrowser.Shared.Models.Common;

public class SelectorOptions
{
    public int Timeout { get; set; } = 30000;
    public string? FrameSelector { get; set; }
}
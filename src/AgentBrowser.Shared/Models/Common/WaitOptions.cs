namespace AgentBrowser.Shared.Models.Common;

public class WaitOptions
{
    public int Timeout { get; set; } = 30000;
    public bool Visible { get; set; } = false;
    public bool Hidden { get; set; } = false;
}
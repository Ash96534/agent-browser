namespace AgentBrowser.Shared.Models;

public class ClickOptions
{
    public int Delay { get; set; } = 0;
    public int Button { get; set; } = 0;
    public int ClickCount { get; set; } = 1;
}

public class TypeOptions
{
    public int Delay { get; set; } = 0;
}

public class WaitForOptions
{
    public int Timeout { get; set; } = 30000;
    public bool Visible { get; set; } = false;
    public bool Hidden { get; set; } = false;
}
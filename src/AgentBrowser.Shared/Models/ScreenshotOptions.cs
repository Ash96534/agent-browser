namespace AgentBrowser.Shared.Models;

public class NavigateOptions
{
    public int? Timeout { get; set; }
    public WaitUntil? WaitUntil { get; set; }
}

public class ScreenshotOptions
{
    public ScreenshotType Type { get; set; } = ScreenshotType.Viewport;
    public string? Selector { get; set; }
    public bool FullPage { get; set; } = false;
    public string Format { get; set; } = "png";
    public int? Quality { get; set; }
}

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
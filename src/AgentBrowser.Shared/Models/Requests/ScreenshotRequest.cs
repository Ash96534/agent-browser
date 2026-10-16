namespace AgentBrowser.Shared.Models.Requests;

public class ScreenshotRequest
{
    public ScreenshotType Type { get; set; } = ScreenshotType.Viewport;
    public string? Selector { get; set; }
    public bool FullPage { get; set; } = false;
    public string Format { get; set; } = "png";
    public int? Quality { get; set; }
}

public enum ScreenshotType
{
    Viewport,
    Element,
    FullPage
}
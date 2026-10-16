namespace AgentBrowser.Shared.Models;

public class Viewport
{
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public double DeviceScaleFactor { get; set; } = 1.0;
    public bool IsMobile { get; set; } = false;
    public bool HasTouch { get; set; } = false;
}
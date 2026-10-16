namespace AgentBrowser.Shared.Models;

public class SessionOptions
{
    public bool Headless { get; set; } = true;
    public Viewport? Viewport { get; set; }
    public string? UserAgent { get; set; }
    public bool IgnoreHTTPSErrors { get; set; } = true;
    public bool DumpIO { get; set; } = false;
    public List<string>? Args { get; set; }
}
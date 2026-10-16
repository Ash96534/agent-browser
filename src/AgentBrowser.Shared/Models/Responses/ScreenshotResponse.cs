namespace AgentBrowser.Shared.Models.Responses;

public class ScreenshotResponse
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string Format { get; set; } = "png";
}
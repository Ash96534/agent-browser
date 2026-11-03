namespace AgentBrowser.Shared.Models.Responses;

public sealed record HtmlResponse
{
    public string Html { get; set; } = string.Empty;
    public string Format { get; set; } = "html";
}
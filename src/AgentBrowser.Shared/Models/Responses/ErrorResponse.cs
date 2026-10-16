namespace AgentBrowser.Shared.Models.Responses;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}
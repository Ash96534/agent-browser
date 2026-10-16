namespace AgentBrowser.Shared.Models.Requests;

public class TypeRequest
{
    public string Selector { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Delay { get; set; } = 0;
    public bool ClearFirst { get; set; } = true;
}
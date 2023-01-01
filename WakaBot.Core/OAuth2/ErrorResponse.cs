namespace WakaBot.Core.OAuth2;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
namespace WakaBot.Core.Models;

public class WakaUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool usingOAuth { get; set; }
    public string? AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public string? Scope { get; set; } = string.Empty;
    public string? State { get; set; } = string.Empty;
    public DiscordUser? DiscordUser { get; set; }
}

using Newtonsoft.Json;

namespace WakaBot.Core.OAuth2;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonIgnore]
    public string State { get; set; } = string.Empty;
}
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace WakaBot.Core.OAuth2;

public class OAuth2Client
{
    private string _name;
    private string _clientId;
    private string _clientSecret;
    private string _authorizeUrl;
    private string _redirectUrl;
    private string _tokenUrl;
    private string? _tokenRevocationUrl;

    private HttpClient _client;

    public OAuth2Client(
        IConfiguration config
    )
    {
        var section = config.GetSection("OAuth2");
        _name = section.GetValue<string>("Name");
        _clientId = section.GetValue<string>("ClientId");
        _clientSecret = section.GetValue<string>("ClientSecret");
        _authorizeUrl = section.GetValue<string>("AuthorizeUrl");
        _redirectUrl = section.GetValue<string>("RedirectUrl");
        _tokenUrl = section.GetValue<string>("TokenUrl");
        _tokenRevocationUrl = section.GetValue<string>("TokenRevocationUrl");

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public string GetRedirectUrl(string[] scopes, string? state = null)
    {
        var query = new Dictionary<string, string?>()
        {
            {"client_id", _clientId},
            {"response_type", "code"},
            {"redirect_uri", _redirectUrl},
            {"scope", string.Join(",", scopes)}
        };
        if (state != null)
        {
            query.Add("state", state);
        }

        return QueryHelpers.AddQueryString(_authorizeUrl, query);
    }

    public async Task<TokenResponse> GetTokenAsync(string code)
    {
        var formData = new Dictionary<string, string>()
        {
            {"code", code},
            {"grant_type", "authorization_code"},
            {"redirect_uri", _redirectUrl},
            {"client_id", _clientId},
            {"client_secret", _clientSecret}
        };
        var res = await _client.PostAsync(_tokenUrl, new FormUrlEncodedContent(formData));

        var tokenData = JsonConvert.DeserializeObject<TokenResponse>(await res.Content.ReadAsStringAsync());
        if (tokenData == null)
        {
            throw new Exception("Request return null response");
        }

        return tokenData;
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var formData = new Dictionary<string, string>()
        {
            {"refresh_token", refreshToken},
            {"grant_type", "refresh_token"},
            {"client_id", _clientId},
            {"client_secret", _clientSecret}
        };
        var res = await _client.PostAsync(_tokenUrl, new FormUrlEncodedContent(formData));

        var tokenData = JsonConvert.DeserializeObject<TokenResponse>(await res.Content.ReadAsStringAsync());
        if (tokenData == null)
        {
            throw new Exception("Request return null response");
        }

        return tokenData;
    }
}
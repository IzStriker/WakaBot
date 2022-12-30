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
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        var res = await client.PostAsync(_tokenUrl, new FormUrlEncodedContent(formData));

        var tokenData = JsonConvert.DeserializeObject<TokenResponse>(await res.Content.ReadAsStringAsync());
        if (tokenData == null)
        {
            throw new Exception("Request return null response");
        }

        return tokenData;
    }
}
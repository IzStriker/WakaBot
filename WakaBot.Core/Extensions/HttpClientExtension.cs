using System.Net.Http.Headers;

namespace WakaBot.Core.Extensions;

public static class HttpClientExtension
{

    public static Task<HttpResponseMessage> GetAsync(
        this HttpClient client,
        string requestUri,
        string accessToken
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }
}
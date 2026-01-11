using RestMiddleware.Extensions;
using System;
using System.Net.Http;

namespace RestMiddleware
{
    public class HttpClientHelper
    {
        public static async System.Threading.Tasks.Task ConfigureHttpClient(HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            client.DefaultRequestHeaders.Add("User-Agent", "RestMiddleware-TestClient/1.0");
            await client.AddTokenAndBaseUrl(options);
        }
    }
}

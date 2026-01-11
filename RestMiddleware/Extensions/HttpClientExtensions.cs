using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RestMiddleware.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient, string url, T data, System.Threading.CancellationToken cancellationToken = default)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await httpClient.PostAsync(url, content, cancellationToken);
        }
        public static async Task<HttpResponseMessage> PostAsMultipartAsync(
            this HttpClient httpClient, string url, FormUrlEncodedContent content, System.Threading.CancellationToken cancellationToken = default)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return await httpClient.PostAsync(url, content, cancellationToken);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }

        public static async Task AddTokenAndBaseUrl(this HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            await AddTokenInHeader(client, options);
            await AddBaseUrl(client, options);
        }
        public static async Task AddTokenInHeader(HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            string token = null;
            if (options.MethodToGetTokenAsync != null)
            {
                token = await options.MethodToGetTokenAsync();
            }
            else if (options.MethodToGetToken != null)
            {
                token = options.MethodToGetToken();
            }

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        public static async Task AddBaseUrl(HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            string baseUrl = null;
            if (options.MethodToGetBaseUrlAsync != null)
            {
                baseUrl = await options.MethodToGetBaseUrlAsync();
            }
            else if (options.MethodToGetBaseUrl != null)
            {
                baseUrl = options.MethodToGetBaseUrl();
            }

            if (!string.IsNullOrEmpty(baseUrl))
                client.BaseAddress = new Uri(baseUrl);
        }

    }
}

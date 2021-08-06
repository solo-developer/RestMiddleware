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
            this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await httpClient.PostAsync(url, content);
        }
        public static async Task<HttpResponseMessage> PostAsMultipartAsync(
            this HttpClient httpClient, string url, FormUrlEncodedContent content)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return await httpClient.PostAsync(url, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }

        public static void AddTokenAndBaseUrl(this HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            AddTokenInHeader(client, options);
            AddBaseUrl(client, options);
        }
        public static void AddTokenInHeader(HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            if (!string.IsNullOrEmpty(options.MethodToGetToken()))
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + options.MethodToGetToken());
        }

        public static void AddBaseUrl(HttpClient client, RestMiddleware.Dto.HttpRequestOptions options)
        {
            client.BaseAddress = new Uri(options.MethodToGetBaseUrl());
        }

    }
}

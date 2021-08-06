using RestMiddleware.Extensions;
using System;
using System.Net.Http;

namespace RestMiddleware
{
    public class HttpClientHelper
    {
        public static HttpClient GetHttpClient(RestMiddleware.Dto.HttpRequestOptions options)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMinutes(3) };
            client.AddTokenAndBaseUrl(options);
            return client;
            //var httpClientHandler = new HttpClientHandler();

            //httpClientHandler.ServerCertificateCustomValidationCallback =
            //(message, cert, chain, errors) => { return true; };

            //return new HttpClient(httpClientHandler);
        }
    }
}

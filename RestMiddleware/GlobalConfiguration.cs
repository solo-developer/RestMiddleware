using RestMiddleware.Dto;

namespace RestMiddleware
{
    public static class GlobalConfiguration
    {
        public static HttpRequestOptions Options { get; } = new HttpRequestOptions();
    }
}

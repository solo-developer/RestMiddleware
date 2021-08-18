using System;

namespace RestMiddleware.Dto
{
    public class HttpRequestOptions
    {
        public Func<string> MethodToGetToken { get; set; }
        public Func<string> MethodToGetBaseUrl { get; set; }
        public Func<string> MethodToGetRefreshTokenEndpoint { get; set; }

        public bool FetchRefreshTokenIfUnauthorised { get; set; }

        public Action<object> MethodToSetTokenLocally { get; set; }
    }
}

using System;

namespace RestMiddleware.Dto
{
    public class HttpRequestOptions
    {
        public Func<string> MethodToGetToken { get; set; }
        public Func<System.Threading.Tasks.Task<string>> MethodToGetTokenAsync { get; set; }
        public Func<string> MethodToGetBaseUrl { get; set; }
        public Func<System.Threading.Tasks.Task<string>> MethodToGetBaseUrlAsync { get; set; }
        public Func<string> MethodToGetRefreshTokenEndpoint { get; set; }

        public Func<string> MethodToGetRefreshToken { get; set; }
        public Func<object> CreateCustomRefreshRequestBody { get; set; }
        public string RefreshTokenParameterName { get; set; } = "refresh_token";
        public string AccessTokenParameterName { get; set; } = "jwt_token";

        public bool FetchRefreshTokenIfUnauthorised { get; set; }

        public Action<object> MethodToSetTokenLocally { get; set; }
        public Func<object, System.Threading.Tasks.Task> MethodToSetTokenLocallyAsync { get; set; }

        /// <summary>
        /// Custom parser for success responses. 
        /// Func(jsonContent, destinationType) returns parsed object.
        /// </summary>
        public Func<string, Type, object> ParseSuccess { get; set; }

        /// <summary>
        /// Custom parser for error responses.
        /// Func(jsonContent) returns list of error messages.
        /// </summary>
        public Func<string, string[]> ParseErrors { get; set; }
    }
}

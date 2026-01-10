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

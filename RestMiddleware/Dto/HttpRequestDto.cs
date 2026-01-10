using System.Collections.Generic;
using System.Net.Http;

namespace RestMiddleware.Dto
{
    public class HttpRequestDto : HttpRequestBaseDto
    {
        public string endpoint { get; set; }
        public string query { get; set; }
        public FormUrlEncodedContent content { get; set; }

        public object data { get; set; }
        
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }

    public class HttpRequestBaseDto
    {
        public HttpRequestBaseDto()
        {
            Options = new HttpRequestOptions();
        }
        public HttpRequestOptions Options { get; set; }

    }
}

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RestMiddleware.Dto
{
    public class HttpResponseDto
    {
        public int StatusCode { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public JObject InfoDetail { get; set; }
        public bool IsSuccess { get => StatusCode == 200; }
        public bool IsError { get => StatusCode == 400; }

        public bool IsInfo { get => !IsSuccess && !IsError; }
    }
}

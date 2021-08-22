using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace RestMiddleware
{
    public class ResponseFormatter
    {
        public static HttpResponseMessage BuildSuccessJson(object data)
        {
            var apiData = new { data };

            var response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK };
            response.Content = new StringContent(JsonConvert.SerializeObject(apiData), System.Text.Encoding.UTF8, "application/json");
            return response;
        }
        public static HttpResponseMessage BuildSuccessJson()
        {
            var apiData = new
            {

            };
            var response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK };
            response.Content = new StringContent(JsonConvert.SerializeObject(apiData), System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage BuildErrorsJson(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var apiMessage = new
            {
                errors = new object[] {
                    new {message = error}
                }
            };

            var response = new HttpResponseMessage() { StatusCode = statusCode };
            response.Content = new StringContent(JsonConvert.SerializeObject(apiMessage), System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage BuildInfoJson(object data, string description, HttpStatusCode statusCode = HttpStatusCode.Continue)
        {
            var apiMessage = new
            {
                data,
                description
            };

            var response = new HttpResponseMessage() { StatusCode = statusCode };
            response.Content = new StringContent(JsonConvert.SerializeObject(apiMessage), System.Text.Encoding.UTF8, "application/json");
            return response;
        }
    }
}

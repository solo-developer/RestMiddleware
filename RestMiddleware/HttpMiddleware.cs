using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestMiddleware.Dto;
using RestMiddleware.Extensions;
using Newtonsoft.Json.Linq;

namespace RestMiddleware.Src
{
    public class HttpMiddleware<T> where T : class
    {

        public static async Task<HttpResponseDto> Post(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);

                if (dto.Options.FetchRefreshTokenIfUnauthorised && IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var response = await GetResponseObject(responseMessage);

                return response;
            }
        }

        public static async Task<(HttpResponseDto response, T data)> PostAndGetObject(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {
                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseDetail = await GetResponseObject(responseMessage);

                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                return (responseDetail, data);
            }
        }

        public static async Task<(HttpResponseDto response, T data)> PostMultipartAndGetObject(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddMultipartAsContentType(client);
                HttpResponseMessage responseMessage = await client.PostAsMultipartAsync(url: dto.endpoint, content: dto.content);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseDetail = await GetResponseObject(responseMessage);
                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                return (responseDetail, data);
            }
        }

        public static async Task<(HttpResponseDto response, List<T> datas)> PostAndGetList(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseDetail = await GetResponseObject(responseMessage);
                List<T> datas = new List<T>();

                if (responseMessage.IsSuccessStatusCode)
                {
                    datas = await JsonParseHelper<List<T>>.GetSuccessObjectByKey(responseMessage);
                }

                return (responseDetail, datas);
            }
        }

        public static async Task<(HttpResponseDto response, List<T> datas)> GetList(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {
                AddJsonAsContentType(client);
                client.MaxResponseContentBufferSize = 2147483647;
                HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseDetail = await GetResponseObject(responseMessage);
                List<T> datas = new List<T>();
                if (responseMessage.IsSuccessStatusCode)
                {
                    datas = await JsonParseHelper<T>.GetSuccessObjects(responseMessage);
                }
                return (responseDetail, datas);
            }
        }

        public static async Task<(HttpResponseDto response, T data)> GetSingleItem(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseObject = await GetResponseObject(responseMessage);
                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                return (responseObject, data);
            }
        }

        public static async Task<(HttpResponseDto response, object data)> GetPrimitiveTypeObject(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseObject = await GetResponseObject(responseMessage);
                object data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                    data = await JsonParseHelper<T>.GetSuccessObject(responseMessage);
                }
                return (responseObject, data);
            }
        }

        public static async Task<HttpResponseDto> DeleteItem(HttpRequestDto dto)
        {
        up:;
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {

                AddJsonAsContentType(client);
                HttpResponseMessage responseMessage = await client.DeleteAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseObject = await GetResponseObject(responseMessage);
                return responseObject;
            }
        }

        public static void AddJsonAsContentType(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected static void AddMultipartAsContentType(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("multipart/form-data"));
        }

        protected static async Task<object> GetJwtTokenUsingRefreshToken(HttpRequestBaseDto dto)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            using (var client = HttpClientHelper.GetHttpClient(dto.Options))
            {
                string jwt_token = dto.Options.MethodToGetToken();
                responseMessage = await client.PostAsJsonAsync(dto.Options.MethodToGetRefreshTokenEndpoint(), new
                {
                    jwt_token = jwt_token
                });

                if (responseMessage.IsSuccessStatusCode)
                {
                    return await JsonParseHelper<object>.GetSuccessObjectByKey(responseMessage);
                }

                string errorMessage = (await JsonParseHelper<T>.GetErrorMessages(responseMessage)).SingleOrDefault();
                throw new Exception(errorMessage);
            }
        }

        private static bool IsUnauthorised(HttpResponseMessage responseMessage)
        {
            return responseMessage.ReasonPhrase.Equals("Unauthorized") && responseMessage.Headers.WwwAuthenticate.Count > 0;
        }

        private static async Task<HttpResponseDto> GetResponseObject(HttpResponseMessage responseMessage)
        {
            int statusCode = (int)responseMessage.StatusCode;
            var response = new HttpResponseDto()
            {
                StatusCode = statusCode
            };

            if (!responseMessage.IsSuccessStatusCode)
            {
                var failureDetail = await GetFailureResponseInformation(responseMessage, response);
                response.Errors = failureDetail.errors?.ToList();
                response.InfoDetail = failureDetail.infoObject;
            }

            return response;
        }

        private static async Task<(JObject infoObject, string[] errors)> GetFailureResponseInformation(HttpResponseMessage responseMessage, HttpResponseDto dto)
        {
            if (dto.IsError)
            {
                var errors = await JsonParseHelper<object>.GetErrorMessages(responseMessage);
                return (null, errors);
            }
            else if (dto.IsInfo)
            {
                var infoObject = await JsonParseHelper<object>.GetInfoObject(responseMessage);
                return (infoObject, null);
            }
            return (null, null);
        }
    }
}

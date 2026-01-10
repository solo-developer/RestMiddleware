using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RestMiddleware.Dto;
using RestMiddleware.Extensions;
using Newtonsoft.Json.Linq;

namespace RestMiddleware.Src
{
    public class RestClient
    {
        private readonly System.Net.Http.IHttpClientFactory _clientFactory;
        private readonly HttpRequestOptions _globalOptions;

        public RestClient(System.Net.Http.IHttpClientFactory clientFactory, HttpRequestOptions globalOptions = null)
        {
            _clientFactory = clientFactory;
            _globalOptions = globalOptions ?? GlobalConfiguration.Options;
        }

        private HttpRequestOptions MergeOptions(HttpRequestOptions requestOptions)
        {
            if (requestOptions.MethodToGetToken == null) requestOptions.MethodToGetToken = _globalOptions.MethodToGetToken;
            if (requestOptions.MethodToGetBaseUrl == null) requestOptions.MethodToGetBaseUrl = _globalOptions.MethodToGetBaseUrl;
            if (requestOptions.MethodToGetRefreshTokenEndpoint == null) requestOptions.MethodToGetRefreshTokenEndpoint = _globalOptions.MethodToGetRefreshTokenEndpoint;
            if (requestOptions.MethodToSetTokenLocally == null) requestOptions.MethodToSetTokenLocally = _globalOptions.MethodToSetTokenLocally;
            
            // Parsers
            if (requestOptions.ParseSuccess == null) requestOptions.ParseSuccess = _globalOptions.ParseSuccess;
            if (requestOptions.ParseErrors == null) requestOptions.ParseErrors = _globalOptions.ParseErrors;

            // If request option is false (default), take global. If true, keep true.
            // This means we can't override global TRUE to FALSE easily without nullable bools, but that's acceptable for now.
            if (!requestOptions.FetchRefreshTokenIfUnauthorised) 
                requestOptions.FetchRefreshTokenIfUnauthorised = _globalOptions.FetchRefreshTokenIfUnauthorised;

            return requestOptions;
        }

        public async Task<HttpResponseDto> Post(HttpRequestDto dto)
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {
                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);

                if (dto.Options.FetchRefreshTokenIfUnauthorised && IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var response = await GetResponseObject(responseMessage, dto.Options);

                return response;
            }
        }

        public async Task<(T data, HttpResponseDto response)> PostAndGetObject<T>(HttpRequestDto dto) where T : class
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {
                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseDetail = await GetResponseObject(responseMessage, dto.Options);

                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     data = (T)dto.Options.ParseSuccess(json, typeof(T));
                }
                else
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                }
                return (data, responseDetail);
            }
        }

        public async Task<(T data, HttpResponseDto response)> PostMultipartAndGetObject<T>(HttpRequestDto dto) where T : class
        {
            MergeOptions(dto.Options);
        up:;
            using (System.Net.Http.HttpClient client = CreateClient(dto.Options))
            {

                AddMultipartAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.PostAsMultipartAsync(url: dto.endpoint, content: dto.content);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseDetail = await GetResponseObject(responseMessage, dto.Options);
                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     data = (T)dto.Options.ParseSuccess(json, typeof(T));
                }
                else
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                }
                return (data, responseDetail);
            }
        }

        public async Task<(List<T> datas, HttpResponseDto response)> PostAndGetList<T>(HttpRequestDto dto) where T : class
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {

                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.PostAsJsonAsync(dto.endpoint, dto.data);
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseDetail = await GetResponseObject(responseMessage, dto.Options);
                List<T> datas = new List<T>();

                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     datas = (List<T>)dto.Options.ParseSuccess(json, typeof(List<T>));
                }
                else
                {
                    datas = await JsonParseHelper<List<T>>.GetSuccessObjectByKey(responseMessage);
                }
                }

                return (datas, responseDetail);
            }
        }

        public async Task<(List<T> datas, HttpResponseDto response)> GetList<T>(HttpRequestDto dto) where T : class
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {
                AddJsonAsContentType(client);
                client.MaxResponseContentBufferSize = 2147483647;
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseDetail = await GetResponseObject(responseMessage, dto.Options);
                List<T> datas = new List<T>();
                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     datas = (List<T>)dto.Options.ParseSuccess(json, typeof(List<T>));
                }
                else
                {
                    datas = await JsonParseHelper<T>.GetSuccessObjects(responseMessage);
                }
                }
                return (datas, responseDetail);
            }
        }

        public async Task<(T data, HttpResponseDto response)> GetSingleItem<T>(HttpRequestDto dto) where T : class
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {

                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseObject = await GetResponseObject(responseMessage, dto.Options);
                T data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     data = (T)dto.Options.ParseSuccess(json, typeof(T));
                }
                else
                {
                    data = await JsonParseHelper<T>.GetSuccessObjectByKey(responseMessage);
                }
                }
                return (data, responseObject);
            }
        }

        public async Task<(object data, HttpResponseDto response)> GetPrimitiveTypeObject(HttpRequestDto dto)
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {

                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.GetAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }

                var responseObject = await GetResponseObject(responseMessage, dto.Options);
                object data = null;
                if (responseMessage.IsSuccessStatusCode)
                {
                if (dto.Options.ParseSuccess != null)
                {
                     var json = await responseMessage.Content.ReadAsStringAsync();
                     data = dto.Options.ParseSuccess(json, typeof(object));
                }
                else
                {
                    data = await JsonParseHelper<object>.GetSuccessObject(responseMessage);
                }
                }
                return (data, responseObject);
            }
        }

        public async Task<HttpResponseDto> DeleteItem(HttpRequestDto dto)
        {
            MergeOptions(dto.Options);
        up:;
            using (var client = CreateClient(dto.Options))
            {

                AddJsonAsContentType(client);
                AddRequestHeaders(client, dto.Headers);
                System.Net.Http.HttpResponseMessage responseMessage = await client.DeleteAsync($"{dto.endpoint}?{dto.query}");
                if (IsUnauthorised(responseMessage))
                {
                    var user = await GetJwtTokenUsingRefreshToken(dto);
                    dto.Options.MethodToSetTokenLocally(user);
                    goto up;
                }
                var responseObject = await GetResponseObject(responseMessage, dto.Options);
                return responseObject;
            }
        }

        private System.Net.Http.HttpClient CreateClient(Dto.HttpRequestOptions options)
        {
            var client = _clientFactory.CreateClient("RestMiddleware");
            HttpClientHelper.ConfigureHttpClient(client, options);
            return client;
        }

        public void AddJsonAsContentType(System.Net.Http.HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected void AddMultipartAsContentType(System.Net.Http.HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("multipart/form-data"));
        }

        private void AddRequestHeaders(System.Net.Http.HttpClient client, Dictionary<string, string> headers)
        {
            if (headers == null) return;
            foreach (var header in headers)
            {
                if (client.DefaultRequestHeaders.Contains(header.Key))
                    client.DefaultRequestHeaders.Remove(header.Key);
                
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        protected async Task<object> GetJwtTokenUsingRefreshToken(HttpRequestBaseDto dto)
        {
            System.Net.Http.HttpResponseMessage responseMessage = new System.Net.Http.HttpResponseMessage();
            using (var client = CreateClient(dto.Options))
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

                string errorMessage = (await JsonParseHelper<object>.GetErrorMessages(responseMessage)).SingleOrDefault();
                throw new Exception(errorMessage);
            }
        }

        private bool IsUnauthorised(System.Net.Http.HttpResponseMessage responseMessage)
        {
            return responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }

        private async Task<HttpResponseDto> GetResponseObject(System.Net.Http.HttpResponseMessage responseMessage, HttpRequestOptions options)
        {
            int statusCode = (int)responseMessage.StatusCode;
            var response = new HttpResponseDto()
            {
                StatusCode = statusCode
            };

            foreach (var header in responseMessage.Headers)
            {
                if (response.Headers.ContainsKey(header.Key))
                {
                    // Append? Replace? Dictionary key must be unique.
                    // The property is Dictionary<string, IEnumerable<string>>.
                    // Headers can have multiple values.
                    // But responseMessage.Headers already groups them.
                    response.Headers[header.Key] = header.Value; 
                }
                else
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (responseMessage.Content != null)
            {
                foreach (var header in responseMessage.Content.Headers)
                {
                    if (response.Headers.ContainsKey(header.Key))
                    {
                         response.Headers[header.Key] = header.Value;
                    }
                    else
                    {
                        response.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                var failureDetail = await GetFailureResponseInformation(responseMessage, response, options);
                response.Errors = failureDetail.errors?.ToList();
                response.InfoDetail = failureDetail.infoObject;
            }

            return response;
        }

        private async Task<(JObject infoObject, string[] errors)> GetFailureResponseInformation(System.Net.Http.HttpResponseMessage responseMessage, HttpResponseDto dto, HttpRequestOptions options)
        {
            if (dto.IsError)
            {
                if (options.ParseErrors != null)
                {
                    var json = await responseMessage.Content.ReadAsStringAsync();
                    var errors = options.ParseErrors(json);
                    return (null, errors);
                }
                var defaultErrors = await JsonParseHelper<object>.GetErrorMessages(responseMessage);
                return (null, defaultErrors);
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

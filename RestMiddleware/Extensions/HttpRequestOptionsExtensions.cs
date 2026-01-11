using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestMiddleware.Src;
using RestMiddleware.Dto;
using System.Linq;
using System.Collections.Generic;

namespace RestMiddleware.Extensions
{
    public static class HttpRequestOptionsExtensions
    {
        public static void UseResponseLayout(this HttpRequestOptions options, string sampleJsonPath, string dataKey, string errorKey)
        {
            ResponseConfigurator.Configure(options, sampleJsonPath, dataKey, errorKey);
        }

        public static void UseSimpleResponse(this HttpRequestOptions options)
        {
            options.ParseSuccess = (json, type) =>
            {
                return JsonConvert.DeserializeObject(json, type);
            };

            options.ParseErrors = (json) =>
            {
                try
                {
                    var token = JToken.Parse(json);
                    if (token is JObject obj)
                    {
                        // Try common error fields
                        if (obj.ContainsKey("errors") && obj["errors"] is JArray errorsArr)
                            return errorsArr.Select(x => x.ToString()).ToArray();
                        if (obj.ContainsKey("errors") && obj["errors"] is JObject errorsObj) // Validation errors like {"Email": ["Required"]}
                            return errorsObj.Properties().SelectMany(p => p.Value.Select(v => $"{p.Name}: {v}")).ToArray();
                        if (obj.ContainsKey("error") && obj["error"].Type == JTokenType.String)
                            return new[] { obj["error"].ToString() };
                        if (obj.ContainsKey("error_description"))
                            return new[] { obj["error_description"].ToString() };
                        if (obj.ContainsKey("message"))
                            return new[] { obj["message"].ToString() };
                        if (obj.ContainsKey("detail"))
                            return new[] { obj["detail"].ToString() };
                        
                        // If no specific key, maybe return full json or nothing? 
                        // Let's fallback to returning the full string if small? 
                        // Or empty array to indicate "Unknown error format"
                    }
                    else if (token is JArray arr)
                    {
                        return arr.Select(x => x.ToString()).ToArray();
                    }
                    else if (token.Type == JTokenType.String)
                    {
                        return new[] { token.ToString() };
                    }
                }
                catch
                {
                    // Not valid JSON, return as raw string
                    if (!string.IsNullOrWhiteSpace(json)) return new[] { json };
                }
                
                return new[] { "Unknown error occurred." };
            };
        }
    }
}

using Newtonsoft.Json.Linq;
using RestMiddleware.Dto;
using System;
using System.IO;
using System.Linq;

namespace RestMiddleware.Src
{
    public static class ResponseConfigurator
    {
        public static void Configure(HttpRequestOptions options, string outputJsonPath, string dataKey, string errorKey)
        {
            if (!File.Exists(outputJsonPath))
            {
                throw new FileNotFoundException($"The sample JSON file was not found at: {outputJsonPath}");
            }

            string jsonContent = File.ReadAllText(outputJsonPath);
            JObject sample = JObject.Parse(jsonContent);

            // Normalize keys (replace > with . for SelectToken)
            string normalizedDataKey = dataKey.Replace(">", ".");
            string normalizedErrorKey = errorKey.Replace(">", ".");

            // validate keys
            if (sample.SelectToken(normalizedDataKey) == null)
            {
                // Warning or Throw? User said "deduce", let's assume valid.
                // But better to fail early if sample is wrong.
                // However, data might be null in sample.
            }

            options.ParseSuccess = (json, type) =>
            {
                var token = JObject.Parse(json).SelectToken(normalizedDataKey);
                return token?.ToObject(type);
            };

            // Deduce error parsing strategy
            var errorToken = sample.SelectToken(normalizedErrorKey);
            bool isSimpleStringArray = false;

            if (errorToken is JArray arr && arr.Count > 0)
            {
                if (arr.First.Type == JTokenType.String)
                {
                    isSimpleStringArray = true;
                }
            }

            if (isSimpleStringArray)
            {
                options.ParseErrors = (json) =>
                {
                    var token = JObject.Parse(json).SelectToken(normalizedErrorKey);
                    return token?.ToObject<string[]>() ?? new string[0];
                };
            }
            else
            {
                // Assume object array with "message" field, as per previous convention
                // Or try to deduce the field name? 
                // Let's check the sample for a "message" property.
                string messageProp = "message";
                if (errorToken is JArray errArr && errArr.First is JObject errObj)
                {
                    if (errObj.ContainsKey("message")) messageProp = "message";
                    else if (errObj.ContainsKey("error")) messageProp = "error";
                    else if (errObj.ContainsKey("msg")) messageProp = "msg";
                }

                options.ParseErrors = (json) =>
                {
                    var root = JObject.Parse(json);
                    var tokens = root.SelectToken(normalizedErrorKey);
                    if (tokens is JArray jArray)
                    {
                        return jArray.Select(x => x[messageProp]?.ToString()).Where(x => x != null).ToArray();
                    }
                    return new string[0];
                };
            }
        }
    }
}

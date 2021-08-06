using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestMiddleware
{
    public class JsonParseHelper<T> where T : class
    {
        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static async Task<List<T>> GetSuccessObjects(HttpResponseMessage response_message, string key = "data")
        {
            var jsonString = await response_message.Content.ReadAsStringAsync();
            JObject joResponse = JObject.Parse(jsonString);
            JArray objectList = (JArray)joResponse[key];
            return objectList.ToObject<List<T>>();
        }

        public static async Task<string[]> GetErrorMessages(HttpResponseMessage responseMessage)
        {
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonString);
            var errors = (JArray)jsonObject["errors"];

            return errors.Select(a => (((JObject)a)["message"]).ToString()).ToArray();
        }

        public static async Task<JObject> GetInfoObject(HttpResponseMessage responseMessage)
        {
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            JObject joResponse = JObject.Parse(jsonString);
            return joResponse;
        }

        public static async Task<T> GetSuccessObjectByKey(HttpResponseMessage response_message, string key = "data")
        {
            var jsonString = await response_message.Content.ReadAsStringAsync();
            JObject joResponse = JObject.Parse(jsonString);

            if (!joResponse.ContainsKey(key))
                return null;

            // if value for object with the specified key is null , we get empty when we convert it to string.
            // In this case, instead of parsing empty string to JObject, we return null
            if (string.IsNullOrEmpty(joResponse[key].ToString()))
                return null;
            JObject successObject = (JObject)joResponse[key];

            T data = JsonConvert.DeserializeObject<T>(successObject.ToString(), jsonSettings);
            return data;
        }
        public static async Task<object> GetSuccessObject(HttpResponseMessage response_message, string key = "data")
        {
            var jsonString = await response_message.Content.ReadAsStringAsync();
            JObject joResponse = JObject.Parse(jsonString);

            if (!joResponse.ContainsKey(key))
                return null;

            // if value for object with the specified key is null , we get empty when we convert it to string.
            // In this case, instead of parsing empty string to JObject, we return null
            if (string.IsNullOrEmpty(joResponse[key].ToString()))
                return null;

            return joResponse[key].ToObject<object>();
        }
    }
}

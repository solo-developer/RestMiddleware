using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestMiddleware.Extensions;
using RestMiddleware.Src;
using RestMiddleware.Dto;

namespace TestClassicFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddRestMiddleware(options =>
            {
                options.MethodToGetBaseUrl = () => "https://jsonplaceholder.typicode.com";
                options.MethodToGetToken = () => ""; 
                
                 options.ParseSuccess = (json, type) => 
                {
                    if (json.TrimStart().StartsWith("["))
                    {
                        var arr = Newtonsoft.Json.Linq.JArray.Parse(json);
                        return arr.ToObject(type);
                    }
                    else
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
                    }
                }; 
            });

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<RestClient>();

            Console.WriteLine("--- Testing GET List (Posts) in .NET Framework 4.8 ---");
            try
            {
                var result = await client.GetList<PostDto>(new HttpRequestDto { endpoint = "/posts" });
                if (result.response.IsSuccess)
                {
                    Console.WriteLine($"Successfully fetched {result.datas.Count} posts.");
                }
                else
                {
                    Console.WriteLine($"Failed: Status {result.response.StatusCode}");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }
    }

    public class PostDto
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
}

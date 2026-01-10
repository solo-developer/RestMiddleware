using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestMiddleware.Extensions;
using RestMiddleware.Src;
using RestMiddleware.Dto;
using System.Collections.Generic;

namespace TestNet5
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddRestMiddleware(options =>
            {
                options.MethodToGetBaseUrl = () => "https://jsonplaceholder.typicode.com"; // JSONPlaceholder returns direct arrays/objects
                options.MethodToGetToken = () => ""; 
                
                // Use Simple Response (Direct Body Parsing)
                options.UseSimpleResponse();
            });

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<RestClient>();

            Console.WriteLine("--- Testing UseSimpleResponse with JSONPlaceholder (List) ---");
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
            
            Console.WriteLine("\n--- Testing UseSimpleResponse (Single) ---");
            try
            {
                var result = await client.GetSingleItem<PostDto>(new HttpRequestDto { endpoint = "/posts/1" });
                if (result.response.IsSuccess)
                {
                    Console.WriteLine($"Successfully fetched post 1.");
                    if(result.data != null) Console.WriteLine($"Title: {result.data.title}");
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

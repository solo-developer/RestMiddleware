using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestMiddleware.Extensions;
using RestMiddleware.Src;
using RestMiddleware.Dto;
using System.IO;

namespace TestNetCore31
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddRestMiddleware(options =>
            {
                options.MethodToGetBaseUrl = () => "https://dummyjson.com"; 
                options.MethodToGetToken = () => ""; 

                // Use the deduced layout
                var schemaPath = Path.Combine(AppContext.BaseDirectory, "response_schema.json");
                // Ensure the file is copied to output or we create it here
                if (!File.Exists(schemaPath))
                {
                    // Fallback create for test
                    File.WriteAllText(schemaPath, "{ \"users\": [] }");
                }
                
                options.UseResponseLayout(schemaPath, "users", "message");
            });

            var serviceProvider = services.BuildServiceProvider();

            // Resolve RestClient
            var client = serviceProvider.GetRequiredService<RestClient>();

            Console.WriteLine("--- Testing GET List (Users) from ReqRes.in ---");
            try
            {
                var result = await client.GetList<UserDto>(new HttpRequestDto 
                { 
                    endpoint = "/users",
                    Headers = new System.Collections.Generic.Dictionary<string, string> 
                    {
                        { "X-Custom-Test", "RESTMiddleware" }
                    }
                });

                if (result.response.IsSuccess)
                {
                    Console.WriteLine($"Successfully fetched {result.datas.Count} users.");
                    Console.WriteLine("Headers found:");
                    foreach (var header in result.response.Headers)
                    {
                        Console.WriteLine($"{header.Key}: {string.Join(",", header.Value)}");
                    }

                    if (result.response.Headers.ContainsKey("Content-Type"))
                    {
                        Console.WriteLine($"Content-Type: {string.Join(",", result.response.Headers["Content-Type"])}");
                    }

                    if (result.datas.Count > 0)
                    {
                        Console.WriteLine($"First User: {result.datas[0].firstName} {result.datas[0].lastName}");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed: Status {result.response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\n--- Testing GET Single Item (User 2) ---");
            try
            {
                var result = await client.GetSingleItem<UserDto>(new HttpRequestDto 
                { 
                    endpoint = "/users/2" 
                });

                if (result.response.IsSuccess)
                {
                    Console.WriteLine($"Successfully fetched user 2.");
                    if (result.data != null)
                        Console.WriteLine($"Name: {result.data.firstName} {result.data.lastName}");
                    else
                        Console.WriteLine("Data is null (expected due to single item not wrapped)");
                }
                else
                {
                    Console.WriteLine($"Failed: Status {result.response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("\n--- Testing Nested Response Layout ---");
            try
            {
                var nestedOptions = new RestMiddleware.Dto.HttpRequestOptions();
                var nestedSchemaPath = Path.Combine(AppContext.BaseDirectory, "nested_schema.json");
                if (!File.Exists(nestedSchemaPath))
                {
                    File.WriteAllText(nestedSchemaPath, "{ \"result\": { \"payload\": { \"items\": [] } }, \"errors\": { \"details\": [] } }");
                }

                nestedOptions.UseResponseLayout(nestedSchemaPath, "result>payload>items", "errors>details");

                // Simulate a response
                string mockJson = "{ \"result\": { \"payload\": { \"items\": [ { \"id\": 99, \"firstName\": \"Nested\", \"lastName\": \"User\" } ] } } }";
                var items = (List<UserDto>)nestedOptions.ParseSuccess(mockJson, typeof(List<UserDto>));
                
                if (items != null && items.Count > 0)
                {
                    Console.WriteLine($"Successfully parsed nested data: {items[0].firstName} {items[0].lastName}");
                }
                else
                {
                    Console.WriteLine("Failed to parse nested data.");
                }

                string mockErrorJson = "{ \"errors\": { \"details\": [ \"Something went wrong deeply\" ] } }";
                var errors = nestedOptions.ParseErrors(mockErrorJson);
                if (errors != null && errors.Length > 0)
                {
                    Console.WriteLine($"Successfully parsed nested error: {errors[0]}");
                }
                else
                {
                    Console.WriteLine("Failed to parse nested error.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nested Test Error: {ex.Message}");
            }
        }
    }

    public class UserDto
    {
        public int id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string image { get; set; }
    }
}

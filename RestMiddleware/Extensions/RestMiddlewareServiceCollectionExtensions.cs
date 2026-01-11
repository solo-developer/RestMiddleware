using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestMiddleware.Dto;
using RestMiddleware.Src;
using System;
using System.Linq;

namespace RestMiddleware.Extensions
{
    public static class RestMiddlewareServiceCollectionExtensions
    {
        public static IServiceCollection AddRestMiddleware(this IServiceCollection services, Action<HttpRequestOptions> configureOptions)
        {
            return services.AddRestMiddleware("Default", configureOptions);
        }

        public static IServiceCollection AddRestMiddleware(this IServiceCollection services, string name, Action<HttpRequestOptions> configureOptions)
        {
            if (string.IsNullOrEmpty(name)) name = "Default";

            // Register the factory if not already registered
            services.TryAddSingleton<IRestClientFactory, RestClientFactory>();
            
            // Register HttpClient if not already registered
            services.AddHttpClient();

            // Register named options as a singleton wrapper
            var options = new HttpRequestOptions();
            configureOptions(options);
            services.AddSingleton(new NamedRestClientOptions(name, options));

            // Optional: Register the "Default" RestClient in DI for convenience
            if (name == "Default")
            {
                services.TryAddTransient<RestClient>(provider => 
                {
                    var factory = provider.GetRequiredService<IRestClientFactory>();
                    return factory.CreateClient("Default");
                });
            }

            return services;
        }
    }

    public class NamedRestClientOptions
    {
        public string Name { get; }
        public HttpRequestOptions Options { get; }

        public NamedRestClientOptions(string name, HttpRequestOptions options)
        {
            Name = name;
            Options = options;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using RestMiddleware.Dto;
using RestMiddleware.Src;
using System;

namespace RestMiddleware.Extensions
{
    public static class RestMiddlewareServiceCollectionExtensions
    {
        public static IServiceCollection AddRestMiddleware(this IServiceCollection services, Action<HttpRequestOptions> configureOptions)
        {
            var options = new HttpRequestOptions();
            configureOptions(options);

            services.AddSingleton(options);
            services.AddHttpClient();
            services.AddTransient<RestClient>();

            return services;
        }
    }
}

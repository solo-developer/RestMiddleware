using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestMiddleware.Src
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEnumerable<Extensions.NamedRestClientOptions> _namedOptions;

        public RestClientFactory(IHttpClientFactory httpClientFactory, IEnumerable<Extensions.NamedRestClientOptions> namedOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _namedOptions = namedOptions ?? throw new ArgumentNullException(nameof(namedOptions));
        }

        public RestClient CreateClient()
        {
            return CreateClient("Default");
        }

        public RestClient CreateClient(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "Default";

            var config = _namedOptions?.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (config != null)
            {
                return new RestClient(_httpClientFactory, config.Options);
            }

            throw new ArgumentException($"No RestMiddleware configuration found with name: {name}");
        }
    }
}

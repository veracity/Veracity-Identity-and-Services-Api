using System;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{

    public static class ClientFactory
    {
        //public static void RegisterTokenProvider(IOAuthTokenProvider provider)
        //{
        //    OauthAttribute.SetOauthProvider(provider);
        //}

        public static void SetServiceProviderFactory(Func<IServiceProvider> factoryMethod)
        {
            _factory = factoryMethod;
        }
        private static Func<IServiceProvider> _factory;

        private static string _baseUrl;
        
        public static IApiClient CreateClient(string baseUrl, IServiceProvider serviceProvider)
        {
            _baseUrl = baseUrl;
            return new ApiClient(baseUrl, serviceProvider ?? _factory?.Invoke());
        }

        internal static IApiClient CreateClient(this ItemReference reference, IServiceProvider serviceProvider)
        {
            return new ApiClient(_baseUrl, serviceProvider ?? _factory?.Invoke());
        }

        internal static IApiClient CreateClient(this UserInfo reference, IServiceProvider serviceProvider)
        {
            return new ApiClient(_baseUrl, serviceProvider ?? _factory?.Invoke());
        }

        internal static IApiClient CreateClient(this ServiceInfo reference, IServiceProvider serviceProvider)
        {
            return new ApiClient(_baseUrl, serviceProvider ?? _factory?.Invoke());
        }
    }
}
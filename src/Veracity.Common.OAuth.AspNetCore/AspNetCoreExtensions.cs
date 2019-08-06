
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Services.Api;

#pragma warning disable 618

namespace Veracity.Common.OAuth.Providers
{
    public static class AspNetCoreExtensions
    {

        /// <summary>
        /// Adds the veracity api services to the IOC container.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="myServicesApiBaseUrl">the base address for the service</param>
        /// <returns></returns>
        public static IServiceCollection AddVeracityServices(this IServiceCollection services, string myServicesApiBaseUrl)
        {
            services.AddInterstellar();
            services.AddScoped(s => s.CreateRestClient<IMy>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IUsersDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IThis>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<ICompaniesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IOptions>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IServicesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IDataContainerService>(myServicesApiBaseUrl));
            services.AddScoped(s => new ApiClientConfiguration(myServicesApiBaseUrl));
            services.AddScoped<IPolicyValidation, PolicyValidation>();
            services.AddSingleton<IDataProtector, DataProtectorNetCore>();
            //Or add a common api accessor for the veracity MyServices api.
            services.AddScoped<IApiClient, ApiClient>();
            return services;
        }


        /// <summary>
        /// Adds the veracity api as controllers for easy access from javascript
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static IMvcBuilder AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(this IMvcBuilder builder, string baseUrl, string authenticationSchemes) where TUserNameResolver : class, IUserNameResolver where TErrorHandler : class, IErrorHandler
        {
            builder.Services.AddScoped<Stardust.Interstellar.Rest.Common.ILogger,InternalLogger>();
            builder.Services
                .AddSingleton<IUserNameResolver, TUserNameResolver>()
                .AddSingleton<IErrorHandler, TErrorHandler>();
            
            ClientFactory.SetServiceProviderFactory(() => builder.Services.BuildServiceProvider());
            builder.Services.SetAuthenticationSchemes(authenticationSchemes);
            builder.AddAsController(s => s.CreateRestClient<IMy>(baseUrl))
                .AddAsController(s => s.CreateRestClient<IThis>(baseUrl))
                .UseInterstellar();
            return builder;
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder, string baseUrl,
            string authenticationSchemes)
        {
            return builder.AddVeracityApiProxies<ErrorHandler, UserNameResolver>(baseUrl, authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder, string authenticationSchemes)
        {
            return builder.AddVeracityApiProxies(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(this IMvcBuilder builder, string authenticationSchemes) where TUserNameResolver : class, IUserNameResolver where TErrorHandler : class, IErrorHandler
        {
            return builder.AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder)
        {
            return builder.AddVeracityApiProxies("Cookies");
        }
    }
}
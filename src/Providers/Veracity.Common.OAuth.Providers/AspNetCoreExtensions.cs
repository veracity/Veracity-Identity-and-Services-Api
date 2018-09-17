using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Reflection;
using Veracity.Services.Api;
using Veracity.Services.Api.Extensions;
#pragma warning disable 618

namespace Veracity.Common.OAuth.Providers
{
    public static class AspNetCoreExtensions
    {

        /// <summary>
        /// Binds the veracity related configuration settings to aspnetcore
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVeracity(this IServiceCollection services)
        {
            ConfigurationManagerHelper.SetManager(new ConfigWrapper());
            services.AddInterstellar().AddSingleton<IWebMethodConverter, VerbResolver>();
            return services;
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key, TokenProviderConfiguration tokenProviderConfiguration)
        {
            ConfigurationManagerHelper.SetManager(new NullConfig());
            services.AddInterstellar().AddSingleton<IWebMethodConverter, VerbResolver>();
            configuration.Bind(key, tokenProviderConfiguration);
            return services;
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddVeracity(configuration, "Veracity", new TokenProviderConfiguration());
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key)
        {
            return services.AddVeracity(configuration, key, new TokenProviderConfiguration());
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key, out TokenProviderConfiguration tokenProviderConfiguration)
        {
            ConfigurationManagerHelper.SetManager(new NullConfig());
            services.AddInterstellar().AddSingleton<IWebMethodConverter, VerbResolver>();
            var t = new TokenProviderConfiguration();
            configuration.Bind(key, t);
            tokenProviderConfiguration = t;
            return services;
        }

        /// <summary>
        /// Binds the veracity related configuration settings to aspnetcore
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVeracity<T>(this IServiceCollection services) where T : IConfigurationReader, new()
        {
            ConfigurationManagerHelper.SetManager(new T());
            services.AddInterstellar().AddSingleton<IWebMethodConverter, VerbResolver>();
            return services;
        }
        /// <summary>
        /// Binds the veracity related configuration settings to aspnetcore
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>

        public static IServiceCollection AddVeracity(this IServiceCollection services, Func<IConfigurationReader> func)
        {
            ConfigurationManagerHelper.SetManager(func.Invoke());
            services.AddSingleton<IWebMethodConverter, VerbResolver>();
            return services;
        }
        /// <summary>
        /// Adds the veracity api services to the IOC container.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="myServicesApiBaseUrl">the base address for the service</param>
        /// <returns></returns>
        public static IServiceCollection AddVeracityServices(this IServiceCollection services, string myServicesApiBaseUrl)
        {
            services.AddScoped(s => s.CreateRestClient<IMy>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IUsersDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IThis>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<ICompaniesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IOptions>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IServicesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IDataContainerService>(myServicesApiBaseUrl));
            services.AddScoped(s => new ApiClientConfiguration(myServicesApiBaseUrl));
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

            builder.Services
                .AddSingleton<IUserNameResolver, TUserNameResolver>()
                .AddSingleton<IErrorHandler, TErrorHandler>();
            ClientFactory.SetServiceProviderFactory(() => builder.Services.BuildServiceProvider());
            builder.Services.SetAuthenticationSchemes(authenticationSchemes);
            builder.AddAsController(s => s.CreateRestClient<IMy>(baseUrl))
                .AddAsController(s => s.CreateRestClient<IThis>(baseUrl))
                .AddAsController(s => s.CreateRestClient<ICompaniesDirectory>(baseUrl))
                .AddAsController(s => s.CreateRestClient<IServicesDirectory>(baseUrl))
                .AddAsController(s => s.CreateRestClient<IUsersDirectory>(baseUrl))
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

    public class VerbResolver : IWebMethodConverter
    {

        public List<HttpMethod> GetHttpMethods(MethodInfo method)
        {
            var verb = method.GetCustomAttribute<VerbAttribute>();
            return new List<HttpMethod> { new HttpMethod(verb?.Verb ?? "GET") };
        }
    }

    internal class NullConfig : IConfigurationReader
    {
        public NameValueCollection AppSettings { get; } = new NameValueCollection();
    }
}
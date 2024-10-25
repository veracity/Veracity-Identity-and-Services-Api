using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    public static class VeracityIdExtensions
    {

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key, TokenProviderConfiguration tokenProviderConfiguration)
        {
            ConfigurationManagerHelper.SetManager(new NullConfig(configuration,key)); 
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
            ConfigurationManagerHelper.SetManager(new NullConfig(configuration,key));

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
            return services;
        }

        public static IServiceCollection AddVeracityDeamonApp(this IServiceCollection services, Func<IConfiguration> func)
        {
            return services.AddVeracityDeamonApp(func.Invoke());
        }
        public static IServiceCollection AddVeracityDeamonApp(this IServiceCollection services, IConfiguration configuration,string key="Veracity")
        {
            ConfigurationManagerHelper.SetManager(new NullConfig(configuration, key));
            var t = new TokenProviderConfiguration();
            configuration.Bind(key, t);
            services.AddDeamoApp(t);
            return services;
        }
        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, Action<AzureAdB2COptions> options)
        {

            builder.AddAzureAdB2C(options);
            return builder;
        }

        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, Func<AuthorizationCodeReceivedContext, Task> additionalAuthCodeHandling = null)
        {
            AzureAdB2CAuthenticationBuilderExtensions.AdditionalAuthCodeHandling = additionalAuthCodeHandling;
            builder.AddVeracityAuthentication(options =>
            {
                configuration.Bind("Veracity", options);

            });
            return builder;
        }

        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, Action<AzureAdB2COptions> options, Func<HttpContext, AuthenticationProperties, bool> isMfaRequiredOptions)
        {

            builder.AddAzureAdB2C(options,isMfaRequiredOptions);
            return builder;
        }

        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, Func<HttpContext, AuthenticationProperties, bool> isMfaRequiredOptions, Func<AuthorizationCodeReceivedContext, Task> additionalAuthCodeHandling = null)
        {
            AzureAdB2CAuthenticationBuilderExtensions.AdditionalAuthCodeHandling = additionalAuthCodeHandling;
            builder.AddVeracityAuthentication(options =>
            {
                configuration.Bind("Veracity", options);

            },isMfaRequiredOptions);
            return builder;
        }

        public static IApplicationBuilder UseVeracity(this IApplicationBuilder app)
        {
            return app;
        }

    }
}
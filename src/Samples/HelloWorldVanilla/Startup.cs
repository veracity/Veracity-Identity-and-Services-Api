using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Security.Claims;
using System.Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth;
using Veracity.Common.OAuth.Providers;
using ILogger = Stardust.Interstellar.Rest.Common.ILogger;
using OwinExtensions = Veracity.Common.Authentication.OwinExtensions;

[assembly: OwinStartup(typeof(HelloWorldVanilla.Startup))]

namespace HelloWorldVanilla
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseLoggingHook(e =>
                {
                    //Write exception to log
                },
                m =>
                {
                    //Write debug message to log
                });
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
                await azureServiceTokenProvider.GetAccessTokenAsync(resource));
            var secret = keyVaultClient.GetSecretAsync("https://veracitydevdaydemo.vault.azure.net/",
                "Veracity1--ClientSecret").Result;
            var subscriptionKey = keyVaultClient
                .GetSecretAsync("https://veracitydevdaydemo.vault.azure.net/", "Veracity--SubscriptionKey").Result;
            app.UseCookieAuthentication(new CookieAuthenticationOptions { CookieName = "a.c" }); //set auth cookie
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType); //set default auth type 
            //configure veracity auth
            app.UseVeracityAuthentication(new TokenProviderConfiguration
            {
                ClientSecret = secret.Value,
                SubscriptionKey = subscriptionKey.Value
            }) //Add Azure Ad B2C authentication and access token cache
                .UseTokenCache(CacheFactoryFunc); //add access token cache and set cache strategy
        }

        private static DistributedTokenCache CacheFactoryFunc()
        {
            return new DistributedTokenCache(HttpContext.Current.User as ClaimsPrincipal, DistributedCache, null, null);
        }

        private static MemoryDistributedCache DistributedCache { get; } =
            new MemoryDistributedCache(
                new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions
                {

                }));
    }

    public class AppServiceConfig : ServicesConfig
    {
        public override bool IncludeProxies => true;

        protected override IServiceCollection Configure(IServiceCollection services)
        {
            services.AddScoped<ILogger, TestLogger>();
            services = base.Configure(services);
            OwinExtensions.SetServiceProviderFactory(()=>services.BuildServiceProvider());
            return services;
        }
    }

    public class TestLogger : ILogger
    {
        public void Error(Exception error)
        {

        }

        public void Message(string message)
        {

        }

        public void Message(string format, params object[] args)
        {

        }
    }
}

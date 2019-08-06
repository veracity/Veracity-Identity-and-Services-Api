# Converting from Veracity.Authentication.OpenIDConnect.* to Veracity.Common.Authentication.*

As we have changed our strategy to provide a unified set of packages that build upon eachother with authientication as the foundation we have deprecated the Veracity.Authentication.OpenIDConnect.* packages. This guide will help in the process of upgrading to the new packages.

We now support different options with regards of token caching through the Microsoft.Extensions.Caching namespace. This provides an easy and standard way of setting up the caching strategy that is most siutable for your solution. Note that persistent token chacing like redis and sql require you to encrupt the tokens before storing them. We also provide options for doing this. 
For details on the new packages see [Readme.md](./readme.md)

## asp.net

This package is implemented using extension methods and not a full owin startup so you are able to customize the behaviour as you like and add additional middlewares if needed. In the samples here we only show the basics.

1. Remove the old package
2. Remove the owin startup reference in web.config
3. install the package: Install-Package Veracity.Common.Authentication.AspNet
4. update web.config keys
    1. veracity:ClientId -> veracity:ClientId
    2. veracity:ClientSecret -> apiGW:clientSecret (should be secured in key vault or the like)
    3. veracity:RedirectUri -> apiGW:redirectUrl
    4. veracity:APISubscriptionKey -> subscriptionKey
5. Add new keys to web.config
    1. apiGW:scope = https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation
    2. apiGW:idp = a68572e3-63ce-4bc1-acdc-b64943502e9d
    3. apiGW:policy = B2C_1A_SignInWithADFSIdp
    4. myApiV3Url = https://api.veracity.com/Veracity/Services/V3 (if calling the Services api using our packages)
6. Add owin startup, see sample below


Sample Owin startup.cs install  Microsoft.Extensions.Caching.Memory -Version 2.0.0 (or higher)

```CS
using System.Security.Claims;
using System.Web;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Veracity.Common.Authentication;

[assembly: OwinStartup(typeof(NetFrameworkIdentity.Startup))]

namespace NetFrameworkIdentity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Example on how to get secrets from key vault
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
                new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
    }
}

```

Modify global.asax.cs

```CS
protected void Application_Start()
{
    ConfigurationManagerHelper.SetManager(new ConfigManager()); //to hook up a config abstraction allowing the shared library work both for asp.net and aspnetcore
    this.AddDependencyInjection<AppServiceConfig>(); //Add Microsoft.Extensions.DependencyInjection support if you dont use DI already. If you are using autofac see the autofac IocIntegration for details
    AreaRegistration.RegisterAllAreas();
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}
```

Sample web.config file

```XML
<appSettings>
    <add key="aspnet:UseTaskFriendlySynchronizationContext" value="true" />
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />
    <add key="apiGW:clientId" value="db4b6456-8873-4358-8c5d-96c39750ec28" />
    <add key="apiGW:policy" value="B2C_1A_SignInWithADFSIdp" />
    <add key="apiGW:scope" value="https://dnvglb2ctest.onmicrosoft.com/a4a8e726-c1cc-407c-83a0-4ce37f1ce130/user_impersonation" />
    <add key="apiGW:redirectUrl" value="https://localhost:44330/" />
    <add key="apiGW:idp" value="ed815121-cdfa-4097-b524-e2b23cd36eb6" />
    <add key="myApiV3Url" value="https://api-test.veracity.com/platform/" />
</appSettings>
```

## ASPNETCORE

1. Remove the old package
2. install the package: Install-Package Veracity.Common.Authentication.AspNetCore
3. Remove ConfigureServices(s=>s.AddSingleton<IVeracityIntegrationConfigService, VeracityIntegrationConfigService>()) from program.cs
4. Remove ConfigureServices(s=>s.AddSingleton<IVeracityOpenIdManager, VeracityOpenIdManager>()) from program.cs
5. Change Constructor, se sample below
6. Replace services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>() with services.AddVeracity(Configuration)
7. Remove: services.AddHttpClient<VeracityPlatformService>();
8. Remove: services.AddSession()
9. Replace AddOpenIdConnect with AddVeracityAuthentication(Configuration)
10. Add token cache, see sample
11. Add DataProtection, see sample

Startup.cs constructor, uses key vault for secrets

```CS
public Startup(IHostingEnvironment env)
{
    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
        .AddAzureKeyVault("https://veracitydevdaydemo.vault.azure.net/", new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)), new DefaultKeyVaultSecretManager())
        .AddEnvironmentVariables();
    Configuration = builder.Build();
}
```

Startup.cs ConfigureServices

```CS
public void ConfigureServices(IServiceCollection services)
{
    services.AddVeracity(Configuration)
        .AddSingleton(ConstructDataProtector)
        .AddSingleton(ConstructDistributedCache).Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    }).AddAuthentication(sharedOptions =>
        {
            sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddVeracityAuthentication(Configuration)
        .AddCookie();


    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
```

Create token cache

```CS
private IDistributedCache ConstructDistributedCache(IServiceProvider s)
{
    return new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
}
```

Create dataprotector

```CS
private IDataProtector ConstructDataProtector(IServiceProvider s)
{
    return new DataProtector<IDataProtectionProvider>(s.GetDataProtectionProvider(), (p, data) => p.CreateProtector("token").Protect(data), (p, data) => p.CreateProtector("token").Unprotect(data));
}
```
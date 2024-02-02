using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api;
using Veracity.Common.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Stardust.Particles;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["KeyVaultEndpoint"]), new DefaultAzureCredential());
 
builder.Services.AddVeracity(builder.Configuration)
                .AddScoped(s => s.GetService<IHttpContextAccessor>().HttpContext.User)
                .AddSingleton(ConstructDistributedCache)
                .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"))
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddVeracityAuthentication(builder.Configuration, isMfaRequiredOptions: (httpContext, authenticationProperties) =>
                {
                    //do custom logic there
                    return true;
                })
                .AddCookie();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseVeracity();
app.UseAuthorization();

app.MapGet("/", (IMy my) => { 
    return $"Welcome {my.Info().Result.Name}";
}).RequireAuthorization();

app.MapGet("/myservices", (IMy my) => {
    return my.MyServices();
}).RequireAuthorization();

app.Run();

IDistributedCache ConstructDistributedCache(IServiceProvider s)
{
    return new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
}
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stardust.Particles;
using System;
using Veracity.Common.OAuth;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api.Extensions;
using IDataProtector = Veracity.Common.OAuth.IDataProtector;
using ILogger = Stardust.Interstellar.Rest.Common.ILogger;

namespace HelloAspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVeracity(Configuration)
                .AddScoped<IOAuthTokenProvider, TokenProvider>()
                .AddSingleton<TokenProviderConfiguration, TokenProviderConfiguration>()
                .AddHttpContextAccessor()
                .AddSingleton<ILogger, LogWrapper>()
                .AddSingleton<ILogging, LogWrapper>()
                .AddScoped(s => s.GetService<IHttpContextAccessor>().HttpContext.User)
                .AddSingleton(ConstructDataProtector)
                .AddSingleton(ConstructDistributedCache)
                .AddScoped<TokenCacheBase, DistributedTokenCache>()
                .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"))
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddVeracityAuthentication(Configuration)
                .AddCookie();
            services.AddMvc()
                .AddVeracityApiProxies(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private IDistributedCache ConstructDistributedCache(IServiceProvider s)
        {
            return new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
        }

        private IDataProtector ConstructDataProtector(IServiceProvider s)
        {
            return new DataProtector<IDataProtectionProvider>(s.GetDataProtectionProvider(), (p, data) => p.CreateProtector("token").Protect(data), (p, data) => p.CreateProtector("token").Unprotect(data));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles()
                .UseVeracity()
                .UseAuthentication()
                .UseMvc();
        }
    }
}

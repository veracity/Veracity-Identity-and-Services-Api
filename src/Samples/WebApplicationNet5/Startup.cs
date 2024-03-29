using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebApplicationNet7.Api;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth.Providers;

namespace WebApplicationNet7
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Stardust.Interstellar.Rest.Service.ServiceFactory.ThrowOnException = true;
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddVeracity(Configuration)
                .AddScoped(s => s.GetService<IHttpContextAccessor>().HttpContext.User)
                .AddSingleton(ConstructDistributedCache)
                .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), services => services.AddScoped(s => s.CreateRestClient<IWtfClient>("https://localhost:63493/")))

                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })

                .AddVeracityAuthentication(Configuration, isMfaRequiredOptions: (httpContext, authenticationProperties) =>
                {
                    //do custom logic there
                    return true;
                })
                .AddCookie();

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .AddAsController<ITestService, TestApi>()
                .AddVeracityApiProxies(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"))
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        private IDistributedCache ConstructDistributedCache(IServiceProvider s)
        {
            return new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
            return s.GetService<IDistributedCache>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseVeracity();
            app.UseRouting().UseAuthorization().UseEndpoints(r =>
            {
                r.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                r.MapRazorPages();
            });
        }
    }
}

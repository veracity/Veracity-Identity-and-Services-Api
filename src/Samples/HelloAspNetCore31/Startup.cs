using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth.Providers;
using IDataProtector = Veracity.Common.Authentication.IDataProtector;

namespace HelloAspNetCore31
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                //.AddAzureKeyVault("https://veracitydevdaydemo.vault.azure.net/", new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)), new DefaultKeyVaultSecretManager())
                .AddEnvironmentVariables();
            Configuration = builder.Build(); 
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
                //.AddSingleton(ConstructDistributedCache)
                //.AddDistributedRedisCache(opt =>
                //{
                //    opt.Configuration = Configuration.GetSection("Veracity").GetValue<string>("RedisConnectionString");
                //    opt.InstanceName = "master3";
                //})
                .AddMemoryCache()
                .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"))
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddVeracityAuthentication(Configuration)
                .AddCookie();

            services.AddMvc(options =>options.EnableEndpointRouting=false ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private IDistributedCache ConstructDistributedCache(IServiceProvider s)
        {
            return s.GetService<IDistributedCache>();
        }

        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

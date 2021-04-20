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
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth;
using Veracity.Common.OAuth.Providers;
using IDataProtector = Veracity.Common.Authentication.IDataProtector;
using ILogger = Veracity.Common.Authentication.ILogger;

namespace HelloAspNetCore
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
				.AddAzureKeyVault("https://veracitydevdaydemo.vault.azure.net/", new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)), new DefaultKeyVaultSecretManager())
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVeracity(Configuration)
                
                .AddSingleton(ConstructDataProtector)
                .AddSingleton(ConstructDistributedCache)
                .AddVeracityServices(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"))
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddVeracityAuthentication(Configuration)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.IsEssential = true;
                } );
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

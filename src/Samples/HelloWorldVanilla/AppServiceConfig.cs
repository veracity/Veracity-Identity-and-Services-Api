using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Common;
using Veracity.Common.OAuth.Providers;
using OwinExtensions = Veracity.Common.Authentication.OwinExtensions;

namespace HelloWorldVanilla
{
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
}
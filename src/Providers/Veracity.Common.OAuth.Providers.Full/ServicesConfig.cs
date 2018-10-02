using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Dependencyinjection;
using System.Configuration;
using Veracity.Services.Api.Extensions;

namespace Veracity.Common.OAuth.Providers
{
    public class ServicesConfig : ServicesConfiguration
    {
        public virtual bool IncludeProxies => false;

        protected override IServiceCollection Configure(IServiceCollection services)
        {
            services.AddScoped<IOAuthTokenProvider,TokenProvider>();
            if (IncludeProxies)
                AddProxies(services);
            services.AddVeracity(ConfigurationManager.AppSettings["myApiV3Url"]);
            return services;
        }

        public IServiceCollection AddProxies(IServiceCollection services, bool doFinalize = true)
        {// typeof(IHttpControllerTypeResolver), new CustomAssebliesResolver()

            return services.AddVeracityProxies(doFinalize);
        }
    }
}

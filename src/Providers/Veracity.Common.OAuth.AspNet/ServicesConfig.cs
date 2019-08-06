#if NET471
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Dependencyinjection;
using System.Configuration;
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth.Providers
{
    public class ServicesConfig : ServicesConfiguration
    {
        public virtual bool IncludeProxies => false;

        protected override IServiceCollection Configure(IServiceCollection services)
        {
            services.AddScoped<ITokenHandler,TokenProvider>();
            if (IncludeProxies)
                AddProxies(services);
            services.AddScoped<IPolicyValidation, PolicyValidation>();
            services.AddVeracity(ConfigurationManager.AppSettings["myApiV3Url"]);
            return services;
        }

        public IServiceCollection AddProxies(IServiceCollection services, bool doFinalize = true)
        {
            return services.AddVeracityProxies(doFinalize);
        }
    }
}

#endif
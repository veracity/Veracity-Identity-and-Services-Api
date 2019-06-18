using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Particles;
using Stardust.Interstellar.Rest.Dependencyinjection;
using Veracity.Common.Authentication;

namespace NetFrameworkIdentity
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigurationManagerHelper.SetManager(new ConfigManager());
            this.AddDependencyInjection<AppServiceConfig>();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public class AppServiceConfig:ServicesConfiguration 
    {
        protected override IServiceCollection Configure(IServiceCollection services)
        {
            return services;
        }
    }
}

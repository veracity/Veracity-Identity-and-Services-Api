using Stardust.Interstellar.Rest.Dependencyinjection;
using Stardust.Particles;
using System;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Veracity.Common.Authentication;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api;

namespace HelloWorldVanilla
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigurationManagerHelper.SetManager(new ConfigManager());
            this.AddDependencyInjection<AppServiceConfig>();//Uses Microsoft.Extensions.DependencyInjection as the IoC container and configures the veracity sdk bindings
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //ClientFactory.RegisterTokenProvider(new TokenProvider());

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            var exception = lastError as ServerException;
            if (exception != null && exception.Status == HttpStatusCode.Unauthorized)
                HttpContext.Current.Response.Redirect("/");
        }
    }
}

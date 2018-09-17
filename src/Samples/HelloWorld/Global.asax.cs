using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Particles;
using Veracity.Services.Api;

namespace HelloWorld
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigurationManagerHelper.SetManager(new ConfigManager());
            ClientFactory.RegisterTokenProvider(new Veracity.Common.OAuth.Providers.TokenProvider());


            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            var exception = lastError as ServerException;
            if (exception != null && exception.Status == HttpStatusCode.Unauthorized)
                HttpContext.Current.Response.Redirect("/");
        }
    }



    public class ConfigManager : IConfigurationReader
    {
        public NameValueCollection AppSettings { get { return ConfigurationManager.AppSettings; } }
    }
}

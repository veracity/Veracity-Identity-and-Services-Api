using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Common.Authentication.AspNet;

namespace HelloWorld
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigurationManagerHelper.SetManager(new ConfigManager());
            //ClientFactory.RegisterTokenProvider(new TokenProvider());//Enable Veracity components to obtain an access token for the logged on  user.
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




}

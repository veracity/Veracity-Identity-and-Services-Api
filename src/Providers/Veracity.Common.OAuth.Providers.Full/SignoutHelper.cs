using System.Web;
using Microsoft.Owin.Security.Cookies;
using Stardust.Particles;
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth.Providers
{
    public static class SignoutHelper
    {
        public static void Logout(this HttpResponseBase response, TokenProviderConfiguration configuration, string redirectUrl, bool isRelative = true)
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(configuration.Policy);
            HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            response.Redirect("https://www.veracity.com/auth/logout");
        }

        public static void Logout(this HttpResponseBase response, string redirectUrl, bool isRelative = true)
        {
            response.Logout(new TokenProviderConfiguration(), redirectUrl, isRelative);
        }
    }
}
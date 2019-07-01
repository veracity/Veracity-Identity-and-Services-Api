using System.Web;
using Microsoft.Owin.Security.Cookies;
using Stardust.Particles;
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth.Providers
{
    public static class SignoutHelper
    {
        public static void Logout(this HttpResponseBase response, TokenProviderConfiguration configuration, string redirectUrl)
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(configuration.Policy);
            HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            response.Redirect(redirectUrl);
        }

        public static void Logout(this HttpResponseBase response, string redirectUrl=null)
        {
            response.Logout(new TokenProviderConfiguration(), redirectUrl?? "https://www.veracity.com/auth/logout");
        }
    }
}
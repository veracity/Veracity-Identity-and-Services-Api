using System.Web;
using Microsoft.Owin.Security.Cookies;
using Stardust.Particles;

namespace Veracity.Common.OAuth.Providers
{
    public static class SignoutHelper
    {
        public static void Logout(this HttpResponseBase response, TokenProviderConfiguration configuration, string redirectUrl, bool isRelative = true)
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(configuration.Policy);
            HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            if (redirectUrl.IsNullOrWhiteSpace()) redirectUrl = "/";
            var logoutUrl = configuration.LogoutUrl ?? $"https://login.microsoftonline.com/{configuration.TenantId}/oauth2/v2.0/logout?p={configuration.Policy}&post_logout_redirect_uri={configuration.RedirectUrl}{redirectUrl}";
            if (TokenProviderConfiguration.DoLogging)
                response.Redirect(logoutUrl);
        }

        public static void Logout(this HttpResponseBase response, string redirectUrl, bool isRelative = true)
        {
            response.Logout(new TokenProviderConfiguration(), redirectUrl, isRelative);
        }
    }
}
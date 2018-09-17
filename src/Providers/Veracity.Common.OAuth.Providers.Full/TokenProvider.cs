using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security.Cookies;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Common.OAuth.Providers
{
    public class TokenProvider : IOAuthTokenProvider
    {
        public TokenProvider() : this(new TokenProviderConfiguration())
        {

        }

        public TokenProvider(TokenProviderConfiguration tokenProviderConfiguration)
        {
            this.tokenProviderConfiguration = tokenProviderConfiguration;
        }

        public override string GetBearerToken()
        {
            var signedInUserID = (HttpContext.Current.User.Identity as ClaimsIdentity)?.FindFirst("userId").Value;
            var cache = GetCache();
            var context = new ConfidentialClientApplication(tokenProviderConfiguration.ClientId, tokenProviderConfiguration.Authority, tokenProviderConfiguration.RedirectUrl, new ClientCredential(tokenProviderConfiguration.ClientSecret), cache, null);
            var user = context.Users.FirstOrDefault();
            if (user == null)
            {
                HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                throw new Veracity.Services.Api.ServerException(new ErrorDetail
                {
                    Message = "Invalid token cache"
                }, System.Net.HttpStatusCode.Unauthorized);
            }
            var token = Task.Run(async () => await context.AcquireTokenSilentAsync(new[] { tokenProviderConfiguration.Scope }, user, tokenProviderConfiguration.Authority, false)).Result;
            return token.CreateAuthorizationHeader();
        }

        public override async Task<string> GetBearerTokenAsync()
        {
            var signedInUserID = (HttpContext.Current.User.Identity as ClaimsIdentity)?.FindFirst("userId").Value;
            var cache = GetCache();
            var context = new ConfidentialClientApplication(tokenProviderConfiguration.ClientId, tokenProviderConfiguration.Authority, tokenProviderConfiguration.RedirectUrl, new ClientCredential(tokenProviderConfiguration.ClientSecret), cache, null);
            var user = context.Users.FirstOrDefault();
            if (user == null)
            {
                HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                throw new Veracity.Services.Api.ServerException(new ErrorDetail
                {
                    Message = "Invalid token cache"
                }, HttpStatusCode.Unauthorized);
            }
            var token = await context.AcquireTokenSilentAsync(new[] { tokenProviderConfiguration.Scope }, user, tokenProviderConfiguration.Authority, false);
            return token.CreateAuthorizationHeader();
        }

        private static TokenCacheBase GetCache()
        {
            return cacheFactory();
        }

        private static Func<TokenCacheBase> cacheFactory;
        private TokenProviderConfiguration tokenProviderConfiguration;
        private object configuration;

        public static void SetCacheFactoryMethod(Func<TokenCacheBase> cacheFactoryFunc)
        {
            cacheFactory = cacheFactoryFunc;
        }
    }
}

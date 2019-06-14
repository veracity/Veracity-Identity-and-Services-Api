using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security.Cookies;
using Stardust.Particles;
using Veracity.Common.OAuth;

namespace Veracity.Common.Authentication.AspNet
{
    public class TokenProvider : ITokenHandler
    {
        //static TokenProvider()
        //{
        //    ConfigurationManagerHelper.SetManager(new ConfigManager());
        //}
        public TokenProvider() : this(new TokenProviderConfiguration())
        {

        }

        public TokenProvider(TokenProviderConfiguration tokenProviderConfiguration)
        {
            this.tokenProviderConfiguration = tokenProviderConfiguration;
        }

        

        public async Task<string> GetBearerTokenAsync()
        {
            var signedInUserID = (HttpContext.Current.User.Identity as ClaimsIdentity)?.FindFirst("userId").Value;
            var cache = GetCache();
            var context = new ConfidentialClientApplication(tokenProviderConfiguration.ClientId, tokenProviderConfiguration.Authority, tokenProviderConfiguration.RedirectUrl, new ClientCredential(tokenProviderConfiguration.ClientSecret), cache, null);
            var user = (await context.GetAccountsAsync()).FirstOrDefault();
            if (user == null)
            {
                HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                throw new ServerException(new ErrorDetail
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

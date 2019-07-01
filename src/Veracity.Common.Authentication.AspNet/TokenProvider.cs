using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security.Cookies;

namespace Veracity.Common.Authentication
{
    public class TokenProvider : ITokenHandler
    {
        public TokenProvider() : this(new TokenProviderConfiguration())
        {

        }

        public TokenProvider(TokenProviderConfiguration tokenProviderConfiguration)
        {
            this.tokenProviderConfiguration = tokenProviderConfiguration;
        }

        

        public async Task<string> GetBearerTokenAsync()
        {
            var cache = GetCache();
            var context = tokenProviderConfiguration.ConfidentialClientApplication(cache, null);
            var user = (await context.GetAccountsAsync()).FirstOrDefault();
            if (user == null)
            {
                HttpContext.Current.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                throw new ServerException(new ErrorDetail
                {
                    Message = "Invalid token cache"
                }, HttpStatusCode.Unauthorized);
            }

            var token = await context.AcquireTokenSilent(new[] {tokenProviderConfiguration.Scope}, user).ExecuteAsync();
            return token.CreateAuthorizationHeader();
        }

        private static TokenCacheBase GetCache()
        {
            return cacheFactory();
        }

        private static Func<TokenCacheBase> cacheFactory;
        private TokenProviderConfiguration tokenProviderConfiguration;

        public static void SetCacheFactoryMethod(Func<TokenCacheBase> cacheFactoryFunc)
        {
            cacheFactory = cacheFactoryFunc;
        }
    }
}

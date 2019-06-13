using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Veracity.Common.OAuth;

namespace Veracity.Common.Authentication.AspNetCore
{
    public class TokenProvider : ITokenHandler
    {
        private readonly TokenProviderConfiguration _configuration;
        private readonly IServiceProvider _appApplicationServices;
        public static string Authority(TokenProviderConfiguration configuration) => $"https://login.microsoftonline.com/tfp/{configuration.TenantId}/{configuration.Policy}/v2.0/.well-known/openid-configuration";
        public TokenProvider() : this(new TokenProviderConfiguration())
        {
        }

        public TokenProvider(IServiceProvider appApplicationServices) : this(appApplicationServices, new TokenProviderConfiguration())
        {
            _appApplicationServices = appApplicationServices;
        }

        private TokenProvider(TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        private TokenProvider(IServiceProvider appApplicationServices, TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetBearerTokenAsync()
        {
            try
            {
                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
                var clientCred = new ClientCredential(_configuration.ClientSecret);
                var context = new ConfidentialClientApplication(_configuration.ClientId, Authority(_configuration), _configuration.RedirectUrl, clientCred, cache, null);
                var user = (await context.GetAccountsAsync()).FirstOrDefault();
                if (user == null)
                {
                    throw new ServerException(new ErrorDetail
                    {
                        Message = "Invalid token cache"
                    }, HttpStatusCode.Unauthorized);
                }
                var token = await context.AcquireTokenSilentAsync(new[] { _configuration.Scope }, user, Authority(_configuration), false);
                return token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
                return null;
            }
        }

        //public static TokenCache TokenCache { get; } = new TokenCache();
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Veracity.Services.Api;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Common.OAuth.Providers
{
    public class TokenProvider : IOAuthTokenProvider
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

        public string GetAccessToken() => GetBearerToken();

        public override string GetBearerToken()
        {
            try
            {
                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
                var signedInUserID = (httpContext.User.Identity as ClaimsIdentity)?.FindFirst("userId").Value;
                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
                //var cache = new MSALSessionCache(signedInUserID, _httpContext.HttpContext.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
                var clientCred = new ClientCredential(_configuration.ClientSecret);
                var context = new ConfidentialClientApplication(_configuration.ClientId, Authority(_configuration), _configuration.RedirectUrl, clientCred, cache, null);
                var user = Task.Run(async () => await context.GetAccountsAsync()).Result.FirstOrDefault();
                if (user == null)
                {
                    throw new ServerException(new ErrorDetail
                    {
                        Message = "Invalid token cache"
                    }, HttpStatusCode.Unauthorized);
                }
                var token = Task.Run(async () => await context.AcquireTokenSilentAsync(new[] { _configuration.Scope }, user, Authority(_configuration), false)).Result;
                return token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
                return null;
            }
        }

        public override async Task<string> GetBearerTokenAsync()
        {
            try
            {
                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
                //var cache = new MSALSessionCache(signedInUserID, _httpContext.HttpContext.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
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
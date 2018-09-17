//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Security.Claims;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Identity.Client;
//using Stardust.Particles;
//using Veracity.Common.OAuth;
//using VerIT.MyDNV.Api.Gateway.ClientModel;
//using VerIT.MyDNV.Api.Gateway.ClientModel.Extensions;
//using VerIT.MyDNV.Api.Gateway.ClientModel.Models;
//using TokenProviderConfiguration = Veracity.Common.OAuth.TokenProviderConfiguration;

//namespace HelloAspNetCore
//{
//    public class TokenProvider : IOAuthTokenProvider
//    {
//        private readonly IServiceProvider _appApplicationServices;
//        public static string Authority => $"https://login.microsoftonline.com/tfp/{TokenProviderConfiguration.TenantId}/{TokenProviderConfiguration.Policy}/v2.0/.well-known/openid-configuration";
//        public TokenProvider()
//        {
//        }

//        public TokenProvider(IServiceProvider appApplicationServices)
//        {
//            _appApplicationServices = appApplicationServices;
//        }

//        public string GetAccessToken() => GetBearerToken();

//        protected override string GetBearerToken()
//        {
//            try
//            {
//                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
//                var signedInUserID = (httpContext.User.Identity as ClaimsIdentity)?.FindFirst("userId").Value;
//                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
//                //var cache = new MSALSessionCache(signedInUserID, _httpContext.HttpContext.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
//                var clientCred = new ClientCredential(TokenProviderConfiguration.ClientSecret);
//                var context = new ConfidentialClientApplication(TokenProviderConfiguration.ClientId, Authority, TokenProviderConfiguration.RedirectUrl, clientCred, cache, null);
//                var user = context.Users.FirstOrDefault();
//                if (user == null)
//                {
//                    throw new ServerException(new ErrorDetail
//                    {
//                        Message = "Invalid token cache"
//                    }, HttpStatusCode.Unauthorized);
//                }
//                var token = Task.Run(async () => await context.AcquireTokenSilentAsync(new[] { TokenProviderConfiguration.Scope }, user, Authority, false)).Result;
//                return token.CreateAuthorizationHeader();
//            }
//            catch (Exception ex)
//            {
//                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
//                return null;
//            }
//        }

//        protected override async Task<string> GetBearerTokenAsync()
//        {
//            try
//            {
//                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
//                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
//                //var cache = new MSALSessionCache(signedInUserID, _httpContext.HttpContext.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
//                var clientCred = new ClientCredential(TokenProviderConfiguration.ClientSecret);
//                var context = new ConfidentialClientApplication(TokenProviderConfiguration.ClientId, Authority, TokenProviderConfiguration.RedirectUrl, clientCred, cache, null);
//                var user = context.Users.FirstOrDefault();
//                if (user == null)
//                {
//                    throw new ServerException(new ErrorDetail
//                    {
//                        Message = "Invalid token cache"
//                    }, HttpStatusCode.Unauthorized);
//                }
//                var token = await context.AcquireTokenSilentAsync(new[] { TokenProviderConfiguration.Scope }, user, Authority, false);
//                return token.CreateAuthorizationHeader();
//            }
//            catch (Exception ex)
//            {
//                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
//                return null;
//            }
//        }

//        //public static TokenCache TokenCache { get; } = new TokenCache();
//    }


//}
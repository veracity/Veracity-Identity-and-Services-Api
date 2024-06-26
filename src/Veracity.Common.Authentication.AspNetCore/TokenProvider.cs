﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Veracity.Common.Authentication
{
    public class TokenProvider : ITokenHandler
    {
        private readonly TokenProviderConfiguration _configuration;
        private readonly IServiceProvider _appApplicationServices;
        public static string Authority(TokenProviderConfiguration configuration) => $"{configuration.Instance}/{configuration.TenantId}/{configuration.Policy}/v2.0/.well-known/openid-configuration";
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

        public async Task<string> GetBearerTokenAsync(string scopes)
        {
            try
            {
                var httpContext = _appApplicationServices.GetService<IHttpContextAccessor>().HttpContext;
                var cache = httpContext.RequestServices.GetService<TokenCacheBase>();
                var context = _configuration.ConfidentialClientApplication(cache, null);
                var user = (await context.GetAccountsAsync()).FirstOrDefault();
                if (user == null)
                {
                    throw new ServerException(new ErrorDetail
                    {
                        Message = "Invalid token cache"
                    }, HttpStatusCode.Unauthorized);
                }
                var token = await context.AcquireTokenSilent(scopes.Split(' '), user).ExecuteAsync();
                return token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
                return null;
            }
        }

    }
    internal class CCTokenProvider : ITokenHandler
    {
        private readonly TokenProviderConfiguration _configuration;
        private readonly IServiceProvider _appApplicationServices;
        public static string Authority(TokenProviderConfiguration configuration) => $"{configuration.Instance}/{configuration.TenantId}/{configuration.Policy}/v2.0/.well-known/openid-configuration";
        public CCTokenProvider() : this(new TokenProviderConfiguration())
        {
        }

        public CCTokenProvider(IServiceProvider appApplicationServices) : this(appApplicationServices, new TokenProviderConfiguration())
        {
            _appApplicationServices = appApplicationServices;
        }

        private CCTokenProvider(TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        private CCTokenProvider(IServiceProvider appApplicationServices, TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetBearerTokenAsync(string scopes)
        {
            try
            {
                
                var context = _configuration.ConfidentialClientApplication(null, null);
               
                var token = await context.AcquireTokenForClient(scopes.Split(' ')).ExecuteAsync();
                return token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
                return null;
            }
        }

    }
}
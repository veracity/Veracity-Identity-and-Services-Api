using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Services.Api.Extensions;

namespace WebApplicationNet7.Api
{
    [Api("api")]
    public interface ITestService:IServiceExtensions
    {
        [Get("")]
        Task<string> Get();
    }

    [Api("api")]
    [MRRTAuth]
    [CircuitBreaker(1000, 2)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
    public interface IWtfClient
    {
        [Get("")]
        Task<string> GetTheThingAsync();
    }

    public class MRRTAuthAttribute : AuthenticationInspectorAttributeBase
    {
        public override IAuthenticationHandler GetHandler(IServiceProvider provider)
        {
            return new AdditionalScopeHandler(provider);
        }

        
    }

    public class AdditionalScopeHandler: IAuthenticationHandler
    {
        private readonly TokenProviderConfiguration _configuration;
        private readonly IServiceProvider _appApplicationServices;
        public static string Authority(TokenProviderConfiguration configuration) => $"{configuration.Instance}/{configuration.TenantId}/{configuration.Policy}/v2.0/.well-known/openid-configuration";
        public AdditionalScopeHandler() : this(new TokenProviderConfiguration())
        {
        }

        public AdditionalScopeHandler(IServiceProvider appApplicationServices) : this(appApplicationServices, new TokenProviderConfiguration())
        {
            _appApplicationServices = appApplicationServices;
        }

        private AdditionalScopeHandler(TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        private AdditionalScopeHandler(IServiceProvider appApplicationServices, TokenProviderConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetBearerTokenAsync()
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
                var token = await context.AcquireTokenSilent(new[] { "https://dnvglb2cprod.onmicrosoft.com/2a9cdff4-d1fc-432d-b99b-34e7cff139df/user_impersonation" }, user).ExecuteAsync();
                return token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                _appApplicationServices.GetService<ILogger<TokenProvider>>().LogError(ex, ex.Message);
                return null;
            }
        }

        public void Apply(HttpRequestMessage req)
        {
            Task.Run(async () => await ApplyAsync(req)).GetAwaiter().GetResult();
        }

        public async Task ApplyAsync(HttpRequestMessage req)
        {
            var token = await GetBearerTokenAsync();
            if (token.ContainsCharacters())
                req.Headers.Add("Authorization", token);
        }

        public void BodyData(byte[] body)
        {
            
        }
    }

    public class TestApi:ITestService
    {
        private Dictionary<string, string> _headers;
        private ControllerContext _context;

        public async Task<string> Get()
        {
            return "Yes, it works!!";
        }

        public void SetControllerContext(ControllerContext currentContext)
        {
            _context = currentContext;
        }

        public void SetResponseHeaderCollection(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }
    }
}

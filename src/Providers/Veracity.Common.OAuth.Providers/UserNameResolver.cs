using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Veracity.Services.Api.Extensions;

namespace Veracity.Common.OAuth.Providers
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger _logger;

        public ErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        private readonly bool _overrideDefaults = true;

        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            if (exception != null) _logger?.Error(exception);
            return null;
        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            if (error != null) _logger?.Error(error);
            return null;
        }

        public bool OverrideDefaults => _overrideDefaults;
    }
    public class UserNameResolver : IUserNameResolver
    {
        private readonly IServiceProvider _locator;

        public UserNameResolver(IServiceProvider locator)
        {
            _locator = locator;
        }
        public string GetCurrentUserName()
        {
            return User?.FindFirst(s => string.Equals(s.Type, ClaimTypes.Upn, StringComparison.InvariantCultureIgnoreCase))?.Value;
        }

        public string GetActorId()
        {
            return User?.FindFirst(c => c.Type == "azp" || c.Type == "appid")
                ?.Value;
        }

        public ClaimsPrincipal User => _locator?.GetService<IHttpContextAccessor>()?.HttpContext?.User;

        public string UserId
        {
            get
            {
                return User?.Claims.SingleOrDefault(s => string.Equals(s.Type, "MyDNVGLGUID", StringComparison.InvariantCultureIgnoreCase))?.Value;

            }
        }
    }
}
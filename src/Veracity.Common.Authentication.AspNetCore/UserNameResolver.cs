using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Veracity.Common.Authentication
{
    
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
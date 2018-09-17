using System;
using System.Linq;
using System.Security.Claims;
using Stardust.Interstellar.Rest.Client;

namespace Veracity.Services.Api
{
    public static class ProxyExtensions
    {
        public static T SetSupportCode<T>(this T service, string supportCode) where T : IVeracityService
        {
            var client = service as RestWrapper;
            client?.SetHttpHeader("x-supportCode", supportCode);
            return service;
        }

        /// <summary>
        /// Generates a support code based on userid and timestamp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static T SetSupportCode<T>(this T service, ClaimsPrincipal principal) where T : IVeracityService
        {
           return service.SetSupportCode($"{principal.Claims.SingleOrDefault(c => string.Equals(c.Type, "dnvglAccountName", StringComparison.InvariantCultureIgnoreCase))?.Value ?? principal.Claims.SingleOrDefault(c => string.Equals(c.Type, "myDnvglGuid", StringComparison.InvariantCultureIgnoreCase))?.Value}_{typeof(T).Name}_{DateTime.UtcNow.Ticks}");
        }
    }
}
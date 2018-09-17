using System;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Stardust.Interstellar.Rest.Common;
using Stardust.Particles.Collection;

namespace Veracity.Common.OAuth
{
    public class DistributedTokenCache : TokenCacheBase
    {



        private readonly ClaimsPrincipal _principal;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly IDataProtector _protector;
        private string _cacheKey;


        TokenCache cache = new TokenCache();

        public DistributedTokenCache(ClaimsPrincipal principal, IDistributedCache distributedCache, ILogger logger, IDataProtector protector)
        {
            _principal = principal;
            _cacheKey = BuildCacheKey(_principal);
            _distributedCache = distributedCache;
            (protector as DataProtector)?.SetLogger(logger);
            _logger = logger;
            _protector = protector;
        }
        private static string BuildCacheKey(ClaimsPrincipal claimsPrincipal)
        {
            string clientId = claimsPrincipal.FindFirst(c => c.Type == "aud")?.Value;
            return string.Format(
                "UserId:{0}",
                claimsPrincipal.FindFirst(c => string.Equals(c.Type, "mydnvglguid", StringComparison.InvariantCultureIgnoreCase))?.Value,
                clientId);
        }


        protected internal override TokenCache GetCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            return cache;
        }



        public void SaveUserStateValue(string state)
        {

        }

        public string ReadUserStateValue()
        {
            string state = string.Empty;

            var binaryState = _distributedCache.Get(_cacheKey + "_state");
            if (binaryState.ContainsElements())
                state = Encoding.UTF8.GetString(binaryState);
            return state;
        }
        public void Load()
        {
            var binaryData = _distributedCache.Get(_cacheKey);
            cache.Deserialize(_protector?.Unprotect(binaryData) ?? binaryData);
        }

        public void Persist()
        {

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            var binaryData = cache.Serialize();
            _distributedCache.Set(_cacheKey, _protector?.Protect(binaryData) ?? binaryData);
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
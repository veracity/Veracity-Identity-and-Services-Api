using System;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;

namespace Veracity.Common.Authentication
{
    public class DistributedTokenCache : TokenCacheBase
    {
        private readonly ClaimsPrincipal _principal;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly IDataProtector _protector;
        private string _cacheKey;

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

        protected internal override ITokenCache GetCacheInstance()
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
            if (binaryState != null && binaryState.Length > 0)
                state = Encoding.UTF8.GetString(binaryState);
            return state;
        }

        public void Load(TokenCacheNotificationArgs args)
        {
            var binaryData = _distributedCache.Get(_cacheKey);
            try
            {
                args.TokenCache.DeserializeMsalV3(_protector?.Unprotect(binaryData) ?? binaryData);
            }
            catch (Exception ex)
            {
                args.TokenCache.DeserializeMsalV2(_protector?.Unprotect(binaryData) ?? binaryData);
                _logger?.Error(ex);
            }
        }

        public void Persist(TokenCacheNotificationArgs args)
        {
            var binaryData = args.TokenCache.SerializeMsalV3();
            _distributedCache.Set(_cacheKey, _protector?.Protect(binaryData) ?? binaryData);
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load(args);
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist(args);
            }
        }
    }
}
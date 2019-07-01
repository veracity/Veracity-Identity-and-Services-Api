using Microsoft.Identity.Client;

namespace Veracity.Common.Authentication
{
    public abstract class TokenCacheBase
    {
        protected ITokenCache cache;
        protected internal abstract ITokenCache GetCacheInstance();

        //public static implicit operator ITokenCache(TokenCacheBase b)
        //{
        //    return b.GetCacheInstance();
        //}

        public void SetCacheInstance(ITokenCache cache)
        {
            this.cache = cache;
            GetCacheInstance();
        }
    }
}
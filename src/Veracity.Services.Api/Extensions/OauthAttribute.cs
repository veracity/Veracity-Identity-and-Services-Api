using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Particles;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Veracity.Services.Api.Extensions
{
    public class OauthAttribute : AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        private IOAuthTokenProvider _provider;

        public OauthAttribute()
        {

        }

        private OauthAttribute(IOAuthTokenProvider iOAuthTokenProvider)
        {
            _provider = iOAuthTokenProvider;
        }

        //public OauthAttribute(string serviceName)
        //{

        //}
        public override IAuthenticationHandler GetHandler(IServiceProvider provider)
        {
            return new OauthAttribute(provider.GetService<IOAuthTokenProvider>());
        }

        public void Apply(HttpWebRequest req)
        {
            req.Headers.Add("Authorization", _provider.GetBearerToken());
            req.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManagerHelper.GetValueOnKey("subscriptionKey"));
        }

        public async Task ApplyAsync(HttpWebRequest req)
        {
            req.Headers.Add("Authorization", await _provider.GetBearerTokenAsync());
            req.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManagerHelper.GetValueOnKey("subscriptionKey"));
        }
    }
}
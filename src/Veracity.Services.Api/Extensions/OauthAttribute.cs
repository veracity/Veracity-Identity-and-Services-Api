using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Particles;
using System;
using System.Net;
using System.Threading.Tasks;
using Veracity.Common.Authentication;

namespace Veracity.Services.Api.Extensions
{
    public class OauthAttribute : AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        private ITokenHandler _provider;

        public OauthAttribute()
        {

        }

        private OauthAttribute(ITokenHandler iOAuthTokenProvider)
        {
            _provider = iOAuthTokenProvider;
        }

        //public OauthAttribute(string serviceName)
        //{

        //}
        public override IAuthenticationHandler GetHandler(IServiceProvider provider)
        {
            return new OauthAttribute(provider.GetService<ITokenHandler>());
        }

        public void Apply(HttpWebRequest req)
        {
            Task.Run(async () =>await  ApplyAsync(req)).GetAwaiter().GetResult();
        }

        public async Task ApplyAsync(HttpWebRequest req)
        {
            req.Headers.Add("Authorization", await _provider.GetBearerTokenAsync());
            req.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManagerHelper.GetValueOnKey("subscriptionKey"));
        }
    }
}
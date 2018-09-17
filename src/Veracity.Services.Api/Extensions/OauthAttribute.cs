using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Particles;
using System.Net;
using System.Threading.Tasks;

namespace Veracity.Services.Api.Extensions
{
    public class OauthAttribute : AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        private static IOAuthTokenProvider _provider;
        //public OauthAttribute(string serviceName)
        //{

        //}
        public override IAuthenticationHandler GetHandler()
        {
            return this;
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

        public static void SetOauthProvider(IOAuthTokenProvider tokenProvider)
        {
            _provider = tokenProvider;

        }
    }
}
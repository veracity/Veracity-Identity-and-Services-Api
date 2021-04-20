using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Particles;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Veracity.Common.Authentication;

namespace Veracity.Services.Api.Extensions
{
    public class OauthAttribute : AuthenticationInspectorAttributeBase, IAuthenticationHandler
    {
        private ITokenHandler _provider;
        private readonly TokenProviderConfiguration _tokenProviderConfiguration;

        public OauthAttribute()
        {

        }

        private OauthAttribute(ITokenHandler iOAuthTokenProvider, TokenProviderConfiguration tokenProviderConfiguration)
        {
            _provider = iOAuthTokenProvider;
            _tokenProviderConfiguration = tokenProviderConfiguration;
        }

        public override IAuthenticationHandler GetHandler(IServiceProvider provider)
        {
            return new OauthAttribute(provider.GetService<ITokenHandler>(),provider.GetService<TokenProviderConfiguration>()??new TokenProviderConfiguration());
        }

        public void Apply(HttpRequestMessage req)
        {
            Task.Run(async () => await ApplyAsync(req)).GetAwaiter().GetResult();
        }

        public async Task ApplyAsync(HttpRequestMessage req)
        {
            var token = await _provider.GetBearerTokenAsync(_tokenProviderConfiguration.Scope);
            if (token.ContainsCharacters())
                req.Headers.Add("Authorization", token);
            req.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManagerHelper.GetValueOnKey("subscriptionKey"));
        }

        public void BodyData(byte[] body)
        {

        }
    }
}
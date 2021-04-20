using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Identity.Client;

namespace Veracity.Common.Authentication
{
    public static class MsalConfidentialAppCreator
    {
        public static IConfidentialClientApplication ConfidentialClientApplication(this TokenProviderConfiguration configuration,
            TokenCacheBase cache, Action<string> _debugLogger)
        {
            var context = ConfidentialClientApplicationBuilder.Create(ClientId(configuration))
                .WithClientSecret(configuration.ClientSecret).WithB2CAuthority(Authority(configuration))//.WithAuthority(Authority(configuration),false)//.WithB2CAuthority(Authority(configuration))
                .WithRedirectUri(configuration.RedirectUrl)
                .WithLogging((level, message, pii) => { _debugLogger?.Invoke(message); })
                .Build();
            cache.SetCacheInstance(context.UserTokenCache);
            return context;
        }
        public static string Authority(TokenProviderConfiguration configuration) => $"{configuration.Instance}{(configuration.Instance.EndsWith("/")?"":"/")}tfp/{TenantId(configuration)}/{configuration.Policy}/v2.0/.well-known/openid-configuration";

        public static string ClientId(TokenProviderConfiguration configuration) => configuration.ClientId;

        public static string DefaultPolicy(TokenProviderConfiguration configuration) => configuration.Policy;
        public static object TenantId(TokenProviderConfiguration configuration) => configuration.TenantId;
        public static object AppUrl(TokenProviderConfiguration configuration) => configuration.RedirectUrl.EndsWith("/") ? configuration.RedirectUrl.Remove(configuration.RedirectUrl.Length - 1, 1) : configuration.RedirectUrl;

    }
}

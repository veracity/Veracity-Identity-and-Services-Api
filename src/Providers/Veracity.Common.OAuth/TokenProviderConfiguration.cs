using Stardust.Particles;

namespace Veracity.Common.OAuth
{
    public class TokenProviderConfiguration
    {

        public string RedirectUrl
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:redirectUrl");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:redirectUrl", value, true);
        }

        public string TenantId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:idp");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:idp", value, true);
        }

        public string ClientSecret
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:clientSecret");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:clientSecret", value, true);
        }

        public string ClientId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:clientId");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:clientId", value, true);
        }

        public string Scope
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:scope");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:scope", value, true);
        }

        public string Policy
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:policy");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:policy", value, true);
        }

        internal object AppUrl => RedirectUrl.EndsWith("/") ? RedirectUrl.Remove(RedirectUrl.Length - 1, 1) : RedirectUrl;
        public static bool DoLogging
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:doLogging", true);
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:doLogging", value.ToString(), true);
        }

        public string Authority => $"https://login.microsoftonline.com/tfp/{TenantId}/{Policy}/v2.0/.well-known/openid-configuration";
        public string ResourceId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:resourceId");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:resourceId", value, true);
        }

        public string ErrorPage
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:errorPage");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:errorPage", value, true);
        }

        public string LogoutUrl
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:logoutUrl");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:logoutUrl", value, true);
        }

        public string MyServicesApi
        {
            get => ConfigurationManagerHelper.GetValueOnKey("myApiV3Url");
            set => ConfigurationManagerHelper.SetValueOnKey("myApiV3Url", value, true);
        }

        public string SubscriptionKey
        {
            get => ConfigurationManagerHelper.GetValueOnKey("subscriptionKey");
            set => ConfigurationManagerHelper.SetValueOnKey("subscriptionKey", value, true);
        }
 
    }
}
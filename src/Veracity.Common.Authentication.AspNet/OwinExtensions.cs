using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Owin;
using Stardust.Particles;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Veracity.Common.Authentication
{
    public static class OwinExtensions
    {

        
        public static void SetServiceProviderFactory(Func<IServiceProvider> factoryMethod)
        {
            ServiceProviderFactory = factoryMethod;
        }

        public static Func<IServiceProvider> ServiceProviderFactory { get; private set; }

        public static IAppBuilder UseTokenCache(this IAppBuilder app, Func<TokenCacheBase> cacheInitialization)
        {
            _tokenCacheCreator = cacheInitialization;
            TokenProvider.SetCacheFactoryMethod(cacheInitialization);
            return app;
        }
        private static Func<TokenCacheBase> _tokenCacheCreator;
        private static Action<Exception> _exceptionLogger;
        private static Action<string> _debugLogger;
        private static IAppBuilder _app;

        public static IAppBuilder ConfigureVeracity(this IAppBuilder app, string environment)
        {
            _app = app;
            var fileContent = File.ReadAllText(HostingEnvironment.MapPath($"~/App_Data/veracity_{environment}.json"));
            var config = JsonConvert.DeserializeObject<TokenProviderConfiguration>(fileContent);
            return app;
        }
        
        public static IAppBuilder UseVeracityAuthentication(this IAppBuilder app, TokenProviderConfiguration configuration)
        {

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // Generate the metadata address using the tenant and policy information
                    MetadataAddress = Authority(configuration),
                    // These are standard OpenID Connect parameters, with values pulled from web.config
                    ClientId = ClientId(configuration),
                    RedirectUri = configuration.RedirectUrl,
                    PostLogoutRedirectUri = configuration.RedirectUrl,
                    ClientSecret = configuration.ClientSecret,
                    ResponseType = "code",
                    // Specify the callbacks for each type of notifications
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        RedirectToIdentityProvider = notification => OnRedirectToIdentityProvider(notification, configuration),
                        AuthorizationCodeReceived = notification => OnAuthorizationCodeReceived(notification, configuration),
                        AuthenticationFailed = notification => OnAuthenticationFailed(notification, configuration),

                    },

                    TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name"
                    },
                    // Specify the scope by appending all of the scopes requested into one string (seperated by a blank space)
                    Scope = $"{OpenIdConnectScopes.OpenId} offline_access {configuration.Scope}"
                }
            );
            return app;
        }

        private static Func<TokenCacheBase> CacheFactoryFunc()
        {
            return _tokenCacheCreator;
        }



        public static string Authority(TokenProviderConfiguration configuration) => $"https://login.microsoftonline.com/tfp/{TenantId(configuration)}/{configuration.Policy}/v2.0/.well-known/openid-configuration";

        public static string ClientId(TokenProviderConfiguration configuration) => configuration.ClientId;

        public static string DefaultPolicy(TokenProviderConfiguration configuration) => configuration.Policy;
        public static object TenantId(TokenProviderConfiguration configuration) => configuration.TenantId;
        public static object AppUrl(TokenProviderConfiguration configuration) => configuration.RedirectUrl.EndsWith("/") ? configuration.RedirectUrl.Remove(configuration.RedirectUrl.Length - 1, 1) : configuration.RedirectUrl;

        private static Task OnRedirectToIdentityProvider(
            RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification, TokenProviderConfiguration configuration)
        {
            var policy = notification.OwinContext.Get<string>("Policy");

            if (!string.IsNullOrEmpty(policy) && !policy.Equals(DefaultPolicy(configuration)))
            {
                notification.ProtocolMessage.Scope = OpenIdConnectScopes.OpenId;
                notification.ProtocolMessage.ResponseType = OpenIdConnectResponseTypes.IdToken;
                notification.ProtocolMessage.IssuerAddress = notification.ProtocolMessage.IssuerAddress.ToLower().Replace(DefaultPolicy(configuration).ToLower(), policy.ToLower());
            }

            return Task.FromResult(0);
        }

        /*
         * Catch any failures received by the authentication middleware and handle appropriately
         */
        private static Task OnAuthenticationFailed(
            AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification, TokenProviderConfiguration configuration)
        {
            notification.HandleResponse();

            // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
            // because password reset is not supported by a "sign-up or sign-in policy"
            if (notification.ProtocolMessage.ErrorDescription != null && notification.ProtocolMessage.ErrorDescription.Contains("AADB2C90118"))
            {
                // If the user clicked the reset password link, redirect to the reset password route
                notification.Response.Redirect("/Account/ResetPassword");
            }
            else if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect(string.Format(configuration.ErrorPage ?? "/error?message={0}", notification.Exception?.Message));
            }

            return Task.FromResult(0);
        }


        /*
         * Callback function when an authorization code is received 
         */
        private static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification, TokenProviderConfiguration configuration)
        {
            var c = HttpContext.Current;
            try
            {

                await ExchangeAuthCodeWithToken(notification, configuration);

                try
                {
                    await ValidatePolicies(notification);
                    notification.OwinContext.Authentication.SignIn(notification.AuthenticationTicket.Identity);
                    notification.Response.Redirect(notification.RedirectUri);
                    notification.HandleResponse();
                    HttpContext.Current = c;
                    
                }
                catch (AggregateException aex)
                {
                    _exceptionLogger?.Invoke(aex);
                    if (aex.InnerException != null)
                    {
                        _exceptionLogger?.Invoke(aex.InnerException);

                    }
                    HttpContext.Current = c;
                    if (aex.InnerException is ServerException serverException)
                        HandlePolicyViolation(notification, serverException);
                }
                catch (ServerException ex)
                {
                    _exceptionLogger?.Invoke(ex);
                    HttpContext.Current = c;
                    HandlePolicyViolation(notification, ex);
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current = c;
                _exceptionLogger?.Invoke(ex);
                throw;
            }
        }

        public static IAppBuilder UseLoggingHook(this IAppBuilder builder, Action<Exception> exceptionLogger,
            Action<string> debugLogger = null)
        {
            _exceptionLogger = exceptionLogger;
            _debugLogger = debugLogger;
            return builder;
        }

        private static async Task ExchangeAuthCodeWithToken(AuthorizationCodeReceivedNotification notification,TokenProviderConfiguration configuration)
        {
            HttpContext.Current.User = new ClaimsPrincipal(notification.AuthenticationTicket.Identity);
             var c = HttpContext.Current;
             var cache = CacheFactoryFunc().Invoke();
            var context = configuration.ConfidentialClientApplication(cache,_debugLogger);
            var user = await context.AcquireTokenByAuthorizationCode(new[] {configuration.Scope}, notification.Code)
                .ExecuteAsync();
            HttpContext.Current = c;
        }

       
        private static async Task ValidatePolicies(AuthorizationCodeReceivedNotification notification)
        {
            _debugLogger?.Invoke($"Validating policies with api: {VeracityApiUrl}");
            var validator = ServiceProviderFactory?.Invoke()?.GetService(typeof(IPolicyValidation)) as IPolicyValidation;
            if (validator != null)
            {
                var policy = ConfigurationManagerHelper.GetValueOnKey("serviceId").ContainsCharacters()
                    ? await validator.ValidatePolicyWithServiceSpesificTerms(ConfigurationManagerHelper.GetValueOnKey("serviceId"),notification.RedirectUri)
                    : await validator.ValidatePolicy(notification.RedirectUri);
                if (!policy.AllPoliciesValid)
                {
                    _debugLogger?.Invoke($"policies validated, redirecting to {policy.RedirectUrl} for approval");
                    notification.Response.Redirect(policy.RedirectUrl);
                    notification.HandleResponse();
                    return;
                }
            }
            _debugLogger?.Invoke($"policies validated");
        }

        private static string VeracityApiUrl => ConfigurationManagerHelper.GetValueOnKey("myApiV3Url");

        private static void HandlePolicyViolation(AuthorizationCodeReceivedNotification notification, ServerException ex)
        {
            if (ex.Status == HttpStatusCode.NotAcceptable)
            {
                notification.Response.Redirect(ex.GetErrorData<ValidationError>().Url);
                notification.HandleResponse();
            }
            else
            {
                _debugLogger?.Invoke(ex.ErrorData);
                _debugLogger?.Invoke(ex.Message);
                _exceptionLogger?.Invoke(ex);
                notification.OwinContext.Authentication.SignIn(notification.AuthenticationTicket.Identity);
            }
        }
    }

    
}

using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;
using Stardust.Particles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Veracity.Services.Api;
using Veracity.Services.Api.Models;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace Veracity.Common.OAuth.Providers
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseTokenCache(this IAppBuilder app, Func<TokenCacheBase> cacheInitialization)
        {
            _tokenCacheCreator = cacheInitialization;
            TokenProvider.SetCacheFactoryMethod(cacheInitialization);
            return app;
        }
        private static Func<TokenCacheBase> _tokenCacheCreator;

        public static IAppBuilder ConfigureVeracity(this IAppBuilder app, string environment)
        {
            var fileContent = File.ReadAllText(HostingEnvironment.MapPath($"~/App_Data/veracity_{environment}.json"));
            var config = JsonConvert.DeserializeObject<TokenProviderConfiguration>(fileContent);
            return app;
        }

        public static ConfigurationWrapper ConfigureVeracity(this IAppBuilder app)
        {
            var fileContent = File.ReadAllText(HostingEnvironment.MapPath($"~/App_Data/veracity.json"));
            var config = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(fileContent);

            return new ConfigurationWrapper(app, config);
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
            RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions>
                notification, TokenProviderConfiguration configuration)
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
                notification.Response.Redirect(string.Format(configuration.ErrorPage, notification.Exception?.Message));
            }

            return Task.FromResult(0);
        }


        /*
         * Callback function when an authorization code is received 
         */
        private static async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification, TokenProviderConfiguration configuration)
        {
            try
            {
                await ExchangeAuthCodeWithToken(notification, configuration);
                try
                {
                    await ValidatePolicies(notification);
                }
                catch (AggregateException aex)
                {
                    if (aex.InnerException is ServerException serverException)
                        HandlePolicyViolation(notification, serverException);
                }
                catch (ServerException ex)
                {
                    HandlePolicyViolation(notification, ex);
                }
            }
            catch (Exception ex)
            {

                //ExtensionsFactory.GetLocator()?.GetService<ILogging>()?.Exception(ex);
                throw;
            }
        }

        private static async Task ExchangeAuthCodeWithToken(AuthorizationCodeReceivedNotification notification,
            TokenProviderConfiguration configuration)
        {
            HttpContext.Current.User = new ClaimsPrincipal(notification.AuthenticationTicket.Identity);
            var cache = CacheFactoryFunc().Invoke();
            var context = new ConfidentialClientApplication(ClientId(configuration), Authority(configuration),
                configuration.RedirectUrl, new ClientCredential(configuration.ClientSecret), cache, null);
            var user = await context.AcquireTokenByAuthorizationCodeAsync(notification.Code,
                new[] { configuration.Scope }); //Keep the user for debugging purposes
        }

        private static async Task ValidatePolicies(AuthorizationCodeReceivedNotification notification)
        {
            await ClientFactory.CreateClient(VeracityApiUrl, new LocatorWrapper(System.Web.Mvc.DependencyResolver.Current))
                .My
                .ValidatePolicies(notification.RedirectUri);
            notification.OwinContext.Authentication.SignIn(notification.AuthenticationTicket.Identity);
            notification.Response.Redirect(notification.RedirectUri);
            notification.HandleResponse();
        }

        private static string VeracityApiUrl => ConfigurationManagerHelper.GetValueOnKey("myApiV3Url");

        private static void HandlePolicyViolation(AuthorizationCodeReceivedNotification notification, ServerException ex)
        {
            if (ex.Status == HttpStatusCode.NotAcceptable)
            {
                notification.Response.Redirect(ex.GetErrorData<ValidationError>().Url);
                notification.HandleResponse();
            }
        }
    }

    internal class LocatorWrapper : IServiceProvider
    {
        private readonly IDependencyResolver _current;

        public LocatorWrapper(IDependencyResolver current)
        {
            _current = current;
        }

        public object GetService(Type serviceType)
        {
            return _current.GetService(serviceType);
        }
    }

    /// <summary>
    /// Add to IOC for proper proxy generation.
    /// </summary>
    public class VerbResolver : IWebMethodConverter
    {

        public List<HttpMethod> GetHttpMethods(MethodInfo method)
        {
            var verb = method.GetCustomAttribute<VerbAttribute>();
            return new List<HttpMethod> { new HttpMethod(verb?.Verb ?? "GET") };
        }
    }
}

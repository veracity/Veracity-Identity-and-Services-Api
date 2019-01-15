using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;
using Veracity.Common.OAuth;
using Veracity.Services.Api;
using Veracity.Services.Api.Models;
using ClientCredential = Microsoft.Identity.Client.ClientCredential;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdB2CAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
            => builder.AddAzureAdB2C(_ => { });

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Action<AzureAdB2COptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect(opts =>
            {
                opts.Scope.Add(TokenProviderConfiguration.Scope);
                opts.Scope.Add("email");
                opts.Scope.Add("offline_access");
                opts.ClientSecret = TokenProviderConfiguration.ClientSecret;
                opts.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                opts.ResponseType = "code id_token";

            });
            //    (options =>
            //{
            //    options.Events.OnAuthorizationCodeReceived = async arg =>
            //    {
            //        try
            //        {
            //            //_logger.LogDebug("Auth code received...");
            //            var cache = new TokenCache();
            //            var context = new ConfidentialClientApplication(ConfigureAzureOptions.ClientId,
            //                ConfigureAzureOptions.Authority, TokenProviderConfiguration.RedirectUrl,
            //                new ClientCredential(TokenProviderConfiguration.ClientSecret), cache, null);
            //            var user = await context.AcquireTokenByAuthorizationCodeAsync(arg.ProtocolMessage.Code,
            //                new[] { TokenProviderConfiguration.Scope });


            //            try
            //            {
            //                await ClientFactory.CreateClient(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url")).My
            //                    .ValidatePolicies(arg.ProtocolMessage.RedirectUri);
            //            }
            //            catch (ServerException ex)
            //            {
            //                //_logger.LogError(ex, "failed to exchange code for token");
            //                if (ex.Status == HttpStatusCode.NotAcceptable)
            //                {
            //                    arg.Response.Redirect(ex.GetErrorData<ValidationError>()
            //                        .Url); //Getting the redirect url from the error message.
            //                    arg.HandleResponse(); //Mark the notification as handled to allow the redirect to happen.
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            throw;
            //        }
            //    };
            //});
            return builder;
        }

        public class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly ILogger _logger;
            private readonly AzureAdB2COptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AzureAdB2COptions> azureOptions, ILogger<ConfigureAzureOptions> logger)
            {
                _logger = logger;
                _logger.LogDebug("Binding events");
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}/{_azureOptions.Domain}/{_azureOptions.SignUpSignInPolicyId}/v2.0";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    OnRemoteFailure = OnRemoteFailure,
                    OnAuthorizationCodeReceived = OneAuthorizationCodeReceived
                };
            }
            public static string Authority => $"https://login.microsoftonline.com/tfp/{TenantId}/{TokenProviderConfiguration.Policy}/v2.0/.well-known/openid-configuration";

            public static string ClientId => TokenProviderConfiguration.ClientId;

            public static string DefaultPolicy => TokenProviderConfiguration.Policy;

            public static object TenantId => TokenProviderConfiguration.TenantId;

            public static object AppUrl => TokenProviderConfiguration.RedirectUrl.EndsWith("/") ? TokenProviderConfiguration.RedirectUrl.Remove(TokenProviderConfiguration.RedirectUrl.Length - 1, 1) : TokenProviderConfiguration.RedirectUrl;

            private async Task OneAuthorizationCodeReceived(AuthorizationCodeReceivedContext arg)
            {
                _logger.LogDebug("Auth code received...");
                try
                {

                    arg.HttpContext.User = arg.Principal;
                    var cache = arg.HttpContext.RequestServices.GetService<TokenCacheBase>();
                    var context = new ConfidentialClientApplication(ClientId, Authority, TokenProviderConfiguration.RedirectUrl, new ClientCredential(TokenProviderConfiguration.ClientSecret), cache, null);
                    var user = await context.AcquireTokenByAuthorizationCodeAsync(arg.ProtocolMessage.Code, new[] { TokenProviderConfiguration.Scope });
                    try
                    {
                        await ClientFactory.CreateClient(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), arg.HttpContext.RequestServices).My.ValidatePolicies(arg.ProtocolMessage.RedirectUri);
                        _logger.LogDebug("Policies validated!");
                    }
                    catch (ServerException ex)
                    {

                        _logger.LogError(ex, "failed to exchange code for token");
                        if (ex.Status == HttpStatusCode.NotAcceptable)
                        {
                            arg.Response.Redirect(ex.GetErrorData<ValidationError>().Url); //Getting the redirect url from the error message.

                            arg.HandleResponse(); //Mark the notification as handled to allow the redirect to happen.
                        }
                        else
                        {
                            arg.HandleCodeRedemption();
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }
            }



            public void Configure(OpenIdConnectOptions options)
            {
                _logger.LogDebug("Configuring");
                Configure(Options.DefaultName, options);
            }

            public Task OnRedirectToIdentityProvider(RedirectContext context)
            {
                _logger.LogDebug("Redirecting");
                var defaultPolicy = _azureOptions.DefaultPolicy;
                if (context.Properties.Items.TryGetValue(AzureAdB2COptions.PolicyAuthenticationProperty, out var policy) &&
                    !policy.Equals(defaultPolicy))
                {
                    context.ProtocolMessage.Scope = OpenIdConnectScope.OpenIdProfile;
                    context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
                    context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.ToLower()
                        .Replace($"/{defaultPolicy.ToLower()}/", $"/{policy.ToLower()}/");
                    context.Properties.Items.Remove(AzureAdB2COptions.PolicyAuthenticationProperty);
                }
                return Task.CompletedTask;
            }

            public Task OnRemoteFailure(RemoteFailureContext context)
            {
                _logger.LogDebug("Failure");
                context.HandleResponse();
                // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
                // because password reset is not supported by a "sign-up or sign-in policy"
                if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
                {
                    // If the user clicked the reset password link, redirect to the reset password route
                    context.Response.Redirect("/Account/ResetPassword");
                }
                else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
                {
                    context.Response.Redirect("/");
                }
                else
                {
                    context.Response.Redirect("/Error");
                }
                return Task.CompletedTask;
            }
        }
    }
    public static class TokenProviderConfiguration
    {

        public static string RedirectUrl
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:redirectUrl");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:redirectUrl", value, true);
        }

        public static string TenantId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:idp");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:idp", value, true);
        }

        public static string ClientSecret
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:clientSecret");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:clientSecret", value, true);
        }

        public static string ClientId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:clientId");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:clientId", value, true);
        }

        public static string Scope
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:scope");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:scope", value, true);
        }

        public static string Policy
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:policy");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:policy", value, true);
        }

        internal static object AppUrl => RedirectUrl.EndsWith("/") ? RedirectUrl.Remove(RedirectUrl.Length - 1, 1) : RedirectUrl;
        public static bool DoLogging
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:doLogging", true);
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:doLogging", value.ToString(), true);
        }

        public static string ApiBaseUrl => ConfigurationManagerHelper.GetValueOnKey("myApiV3Url");

        public static string Authority => $"https://login.microsoftonline.com/tfp/{TenantId}/{Policy}/v2.0/.well-known/openid-configuration";
        public static string ResourceId
        {
            get => ConfigurationManagerHelper.GetValueOnKey("apiGW:resourceId");
            set => ConfigurationManagerHelper.SetValueOnKey("apiGW:resourceId", value, true);
        }
    }

}

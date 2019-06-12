using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;
using Veracity.Services.Api;
using Veracity.Services.Api.Models;

namespace Veracity.Common.OAuth.Providers
{
    internal static class AzureAdB2CAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
            => builder.AddAzureAdB2C(_ => { }, new TokenProviderConfiguration());

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder,
            Action<AzureAdB2COptions> configureOptions)
        {
            return builder.AddAzureAdB2C(configureOptions, new TokenProviderConfiguration());
        }
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Action<AzureAdB2COptions> configureOptions, TokenProviderConfiguration configuration)
        {
            builder.Services.Configure(configureOptions);
            
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect(opts =>
            {
                
                opts.Scope.Add(configuration.Scope);
                opts.Scope.Add("email");
                opts.Scope.Add("offline_access");
                opts.ClientSecret = configuration.ClientSecret;
                opts.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                opts.ResponseType = "code id_token";
            });

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

            internal void Configure(string name, OpenIdConnectOptions options, TokenProviderConfiguration configuration)
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
                    OnAuthorizationCodeReceived = context => OneAuthorizationCodeReceived(context, configuration)
                };
            }
            public static string Authority(TokenProviderConfiguration configuration) => $"https://login.microsoftonline.com/tfp/{TenantId(configuration)}/{configuration.Policy}/v2.0/.well-known/openid-configuration";

            public static string ClientId(TokenProviderConfiguration configuration) => configuration.ClientId;

            public static string DefaultPolicy(TokenProviderConfiguration configuration) => configuration.Policy;

            public static string TenantId(TokenProviderConfiguration configuration) => configuration.TenantId;

            public static string AppUrl(TokenProviderConfiguration configuration) => configuration.RedirectUrl.EndsWith("/") ? configuration.RedirectUrl.Remove(configuration.RedirectUrl.Length - 1, 1) : configuration.RedirectUrl;

            private async Task OneAuthorizationCodeReceived(AuthorizationCodeReceivedContext arg, TokenProviderConfiguration configuration)
            {
                _logger.LogDebug("Auth code received...");
                try
                {

                    arg.HttpContext.User = arg.Principal;
                    var cache = arg.HttpContext.RequestServices.GetService<TokenCacheBase>();
                    var context = new ConfidentialClientApplication(ClientId(configuration), Authority(configuration), configuration.RedirectUrl, new ClientCredential(configuration.ClientSecret), cache, null);
                    var user = await context.AcquireTokenByAuthorizationCodeAsync(arg.ProtocolMessage.Code, new[] { configuration.Scope });

                    try
                    {
                        await ClientFactory.CreateClient(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"),null).My
                            .SetSupportCode(
                                $"{arg.Principal.Claims.SingleOrDefault(c => string.Equals(c.Type, "dnvglAccountName", StringComparison.InvariantCultureIgnoreCase))?.Value ?? arg.Principal.Claims.SingleOrDefault(c => string.Equals(c.Type, "myDnvglGuid", StringComparison.InvariantCultureIgnoreCase))?.Value}_login_{DateTime.UtcNow.Ticks}")
                            .ValidatePolicies(arg.ProtocolMessage.RedirectUri);
                        _logger.LogDebug("Policies validated!");
                    }
                    catch (AggregateException aex)
                    {
                        var e = aex.InnerException as ServerException;
                        if (e != null)
                        {
                            HandleServerException(arg, e);
                        }
                    }
                    catch (ServerException ex)
                    {
                        HandleServerException(arg, ex);
                    }

                }
                catch (Exception ex)
                {
                    ex.Log();
                    arg.HandleCodeRedemption();
                }
            }

            private void HandleServerException(AuthorizationCodeReceivedContext arg, ServerException e)
            {
                _logger.LogError(e, "failed to exchange code for token");
                if (e.Status == HttpStatusCode.NotAcceptable)
                {
                    arg.Response.Redirect(e.GetErrorData<ValidationError>().Url); //Getting the redirect url from the error message.

                    arg.HandleResponse(); //Mark the notification as handled to allow the redirect to happen.
                }
                else
                {
                    arg.HandleCodeRedemption();
                }
            }


            internal void Configure(OpenIdConnectOptions options, TokenProviderConfiguration configuration)
            {
                _logger.LogDebug("Configuring");
                Configure(Options.DefaultName, options, configuration);
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

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(options, new TokenProviderConfiguration());
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                Configure(name, options, new TokenProviderConfiguration());
            }
        }
    }
}
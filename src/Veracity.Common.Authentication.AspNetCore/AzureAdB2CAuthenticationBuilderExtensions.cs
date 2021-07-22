using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    internal static class AzureAdB2CAuthenticationBuilderExtensions
    {
        private static AuthenticationBuilder _builder;
        private static bool _upgradeHttp;


        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
            => builder.AddAzureAdB2C(_ => { }, new TokenProviderConfiguration());

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder,
            Action<AzureAdB2COptions> configureOptions)
        {
            return builder.AddAzureAdB2C(configureOptions, new TokenProviderConfiguration());
        }
        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Action<AzureAdB2COptions> configureOptions, TokenProviderConfiguration configuration)
        {
            _builder = builder;
            builder.Services.Configure(configureOptions);

            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>()
                .AddScoped<ITokenHandler, TokenProvider>()
                .AddSingleton<TokenProviderConfiguration, TokenProviderConfiguration>()
                .AddHttpContextAccessor()
                .AddSingleton<ILogger, LogWrapper>()
                .AddSingleton<ILogging, LogWrapper>()
                .AddScoped(s => s.GetService<IHttpContextAccessor>().HttpContext.User)

                .AddScoped<TokenCacheBase>(s => new DistributedTokenCache(s.GetService<IHttpContextAccessor>().HttpContext.User, s.GetService<IDistributedCache>(), s.GetService<ILogger>(), s.GetService<IDataProtector>()));
            builder.AddOpenIdConnect(opts =>
            {
                _upgradeHttp = configuration.UpgradeHttp;
                opts.Scope.Add(configuration.Scope);
                opts.Scope.Add("openid");
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

            public ConfigureAzureOptions(IOptions<AzureAdB2COptions> azureOptions, ILogger logger)
            {
                _logger = logger;
                _logger?.Message("Binding events");
                _azureOptions = azureOptions.Value;
            }

            internal void Configure(string name, OpenIdConnectOptions options, TokenProviderConfiguration configuration)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}/{_azureOptions.Domain}/{_azureOptions.SignUpSignInPolicyId}/v2.0";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
                var handler = options.Events.OnAuthorizationCodeReceived;
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context => OnRedirectToIdentityProvider(context, configuration),
                    OnRemoteFailure = OnRemoteFailure,
                    OnAuthorizationCodeReceived = context => OnAuthorizationCodeReceived(context, configuration, handler)
                };
            }

            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext arg, TokenProviderConfiguration configuration, Func<AuthorizationCodeReceivedContext, Task> handler)
            {
                _logger?.Message("Auth code received...");
                var timer = Stopwatch.StartNew();
                try
                {
                    arg.HttpContext.User = arg.Principal;
                    if (configuration.RequireMfa && !arg.Principal.Claims.Any(c => c.Type == "mfa_required" && c.Value == "true"))
                        throw new UnauthorizedAccessException("MFA required");
                    var cache = arg.HttpContext.RequestServices.GetService<TokenCacheBase>();
                    var context = configuration.ConfidentialClientApplication(cache, s => { _logger?.Message(s); });
                    var user = await context.AcquireTokenByAuthorizationCode(new[] { configuration.Scope }, arg.ProtocolMessage.Code).ExecuteAsync();
                    _logger?.Message($"exchanging code with access token took: {timer.ElapsedMilliseconds}ms");
                    var policyValidator = arg.HttpContext.RequestServices.GetService<IPolicyValidation>();
                    try
                    {
                        if (policyValidator != null)
                        {
                            var timer2 = Stopwatch.StartNew();
                            var policy = await ValidatePolicies(configuration, policyValidator, arg.ProtocolMessage.RedirectUri ?? configuration.PolicyRedirectUrl??configuration.RedirectUrl);
                            timer2.Stop();
                            _logger?.Message($"Policy check took {timer2.ElapsedMilliseconds}ms. ");
                            if (policy.AllPoliciesValid)
                            {
                                _logger?.Message("Policies validated!");
                                AdditionalAuthCodeHandling?.Invoke(arg);
                            }
                            else
                            {
                                _logger?.Message("Not all policies is valid, redirecting to Veracity");
                                arg.Response.Redirect(policy.RedirectUrl); //Getting the redirect url from the error message.
                                arg.HandleResponse();
                            }
                        }
                        else
                        {
                            AdditionalAuthCodeHandling?.Invoke(arg);
                        }
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
                }
                timer.Stop();
                _logger?.Message($"Total on code received  took {timer.ElapsedMilliseconds}ms. ");
                await handler(arg);
            }

            private static async Task<ValidationResult> ValidatePolicies(TokenProviderConfiguration configuration,
                IPolicyValidation policyValidator, string protocolMessageRedirectUri)
            {
                var policy = configuration.ServiceId != null
                    ? await policyValidator.ValidatePolicyWithServiceSpesificTerms(configuration.ServiceId, protocolMessageRedirectUri)
                    : await policyValidator.ValidatePolicy(protocolMessageRedirectUri);
                return policy;
            }

            private void HandleServerException(AuthorizationCodeReceivedContext arg, ServerException e)
            {
                _logger?.Error(e);
                if (e.Status == HttpStatusCode.NotAcceptable)
                {
                    arg.Response.Redirect(e.GetErrorData<ValidationError>().Url); //Getting the redirect url from the error message.

                }
            }


            internal void Configure(OpenIdConnectOptions options, TokenProviderConfiguration configuration)
            {
                _logger?.Message("Configuring");
                Configure(Options.DefaultName, options, configuration);
            }

            public Task OnRedirectToIdentityProvider(RedirectContext context,TokenProviderConfiguration configuration)
            {
                _logger?.Message("Redirecting");
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
                if (_upgradeHttp)
                {
                    if (context.ProtocolMessage.RedirectUri.StartsWith("http://"))
                        context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://");
                }
                if (configuration.RequireMfa || (context.Properties.Items.TryGetValue("RequireMfa",out var requireMfa) && requireMfa == "true"))
                    context.ProtocolMessage.SetParameter("mfa_required", "true");
                return Task.CompletedTask;
            }

            public Task OnRemoteFailure(RemoteFailureContext context)
            {
                _logger?.Message("Failure");
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

        public static Func<AuthorizationCodeReceivedContext, Task> AdditionalAuthCodeHandling { get; internal set; }
    }
}

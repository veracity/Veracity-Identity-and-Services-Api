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
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    internal static class AzureAdB2CAuthenticationBuilderExtensions
    {
        private static AuthenticationBuilder _builder;
        private static bool _upgradeHttp;

        private static Func<HttpContext, AuthenticationProperties, bool> _conditionalMfaFunc;

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder)
            => builder.AddAzureAdB2C(_ => { }, new TokenProviderConfiguration());

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder,
            Action<AzureAdB2COptions> configureOptions)
        {
            return builder.AddAzureAdB2C(configureOptions, new TokenProviderConfiguration());
        }

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder, Func<HttpContext, AuthenticationProperties, bool> isMfaRequiredOptions)
        {
            _conditionalMfaFunc = isMfaRequiredOptions;
            return builder.AddAzureAdB2C(_ => { }, new TokenProviderConfiguration());
        }

        public static AuthenticationBuilder AddAzureAdB2C(this AuthenticationBuilder builder,
            Action<AzureAdB2COptions> configureOptions, Func<HttpContext, AuthenticationProperties, bool> isMfaRequiredOptions)
        {
            _conditionalMfaFunc = isMfaRequiredOptions;
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

        internal static IServiceCollection AddDeamoApp(this IServiceCollection builder, TokenProviderConfiguration configuration)
        {
            builder
                .AddScoped<ITokenHandler, CCTokenProvider>()
                .AddSingleton<TokenProviderConfiguration, TokenProviderConfiguration>()
                .AddSingleton<ILogger, LogWrapper>()
                .AddSingleton<ILogging, LogWrapper>();


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
                ValidateAzureOptions(_azureOptions);
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}/{_azureOptions.Domain}/{_azureOptions.SignUpSignInPolicyId}/v2.0";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;

                options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
                var userProvided = _azureOptions.OpenIdConnectEvents;

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = async context =>
                    {
                        await OnRedirectToIdentityProvider(context, configuration);
                        await userProvided.OnRedirectToIdentityProvider(context);
                    },
                    OnRemoteFailure = async context =>
                    {
                        await OnRemoteFailure(context);
                        await userProvided.OnRemoteFailure(context);
                    },
                    OnAuthorizationCodeReceived = async context =>
                    {
                        await OnAuthorizationCodeReceived(context, configuration);
                        await userProvided.OnAuthorizationCodeReceived(context);
                    },
                    OnTokenValidated = async context =>
                    {
                        await userProvided.OnTokenValidated(context);
                    },
                    OnAccessDenied = async context =>
                    {
                        await userProvided.OnAccessDenied(context);
                    },
                    OnAuthenticationFailed = async context =>
                    {
                        await userProvided.OnAuthenticationFailed(context);
                    },
                    OnMessageReceived = async context =>
                    {
                        await userProvided.OnMessageReceived(context);
                    },
                    OnRedirectToIdentityProviderForSignOut = async context =>
                    {
                        await userProvided.OnRedirectToIdentityProviderForSignOut(context);
                    },
                    OnRemoteSignOut = async context =>
                    {
                        await userProvided.OnRemoteSignOut(context);
                    },
                    OnSignedOutCallbackRedirect = async context =>
                    {
                        await userProvided.OnSignedOutCallbackRedirect(context);
                    },
                    OnTicketReceived = async context =>
                    {
                        await userProvided.OnTicketReceived(context);
                    },
                    OnTokenResponseReceived = async context =>
                    {
                        await userProvided.OnTokenResponseReceived(context);
                    },
                    OnUserInformationReceived = async context =>
                    {
                        await userProvided.OnUserInformationReceived(context);
                    }
                };
            }

            private static void ValidateAzureOptions(AzureAdB2COptions azureOptions)
            {
                if (azureOptions == null) throw new ArgumentNullException(nameof(azureOptions));
                ValidationHelper.ValidateRequiredString(azureOptions.ClientId, nameof(azureOptions.ClientId));
                ValidationHelper.ValidateRequiredString(azureOptions.Instance, nameof(azureOptions.Instance));
                ValidationHelper.ValidateRequiredString(azureOptions.Domain, nameof(azureOptions.Domain));
                ValidationHelper.ValidateRequiredString(azureOptions.SignUpSignInPolicyId, nameof(azureOptions.SignUpSignInPolicyId));
                ValidationHelper.ValidateRequiredString(azureOptions.CallbackPath, nameof(azureOptions.CallbackPath));
            }

            private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext arg, TokenProviderConfiguration configuration)
            {
                _logger?.Message("Auth code received...");
                var timer = Stopwatch.StartNew();
                try
                {
                    arg.HttpContext.User = arg.Principal;
                    if (IsMfaRequired(arg, configuration) &&
                        !arg.Principal.Claims.Any(c => c.Type == "mfa_required" && c.Value == "true"))
                        throw new UnauthorizedAccessException("MFA required");
                    var cache = arg.HttpContext.RequestServices.GetService<TokenCacheBase>();
                    var context = configuration.ConfidentialClientApplication(cache, s => { _logger?.Message(s); });
                    var user = await context
                        .AcquireTokenByAuthorizationCode(new[] { configuration.Scope }, arg.ProtocolMessage.Code)
                        .ExecuteAsync();
                    _logger?.Message($"exchanging code with access token took: {timer.ElapsedMilliseconds}ms");
                    var policyValidator = arg.HttpContext.RequestServices.GetService<IPolicyValidation>();
                    try
                    {
                        if (policyValidator != null)
                        {
                            var timer2 = Stopwatch.StartNew();
                            var policy = await ValidatePolicies(configuration, policyValidator,
                                arg.ProtocolMessage.RedirectUri ??
                                configuration.PolicyRedirectUrl ?? configuration.RedirectUrl);
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
                                arg.Response.Redirect(policy
                                    .RedirectUrl); //Getting the redirect url from the error message.
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
                        else if (AzureAdB2COptions.TerminateOnPolicyException) throw new Exception(aex.Message, aex);
                    }
                    catch (ServerException ex)
                    {
                        HandleServerException(arg, ex);
                    }

                }
                catch (MsalClientException ex)
                {
                    _logger?.Message(ex.Message);
                    _logger?.Error(ex);
                    if (AzureAdB2COptions.TerminateOnPolicyException) throw new Exception(ex.Message, ex);
                }
                catch (MsalServiceException ex)
                {
                    _logger?.Message(ex.ResponseBody);
                    _logger?.Message(ex.Message);
                    _logger?.Message(ex.Claims ?? "");
                    _logger?.Error(ex);
                    if (AzureAdB2COptions.TerminateOnPolicyException) throw new Exception(ex.Message, ex);
                }
                catch (Exception ex)
                {
                    ex.Log();
                    if (AzureAdB2COptions.TerminateOnPolicyException) throw new Exception(ex.Message, ex);
                }
                timer.Stop();
                _logger?.Message($"Total on code received  took {timer.ElapsedMilliseconds}ms. ");
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
                else if (AzureAdB2COptions.TerminateOnPolicyException) throw new Exception(e.Message, e);
            }


            internal void Configure(OpenIdConnectOptions options, TokenProviderConfiguration configuration)
            {
                _logger?.Message("Configuring");
                Configure(Options.DefaultName, options, configuration);
            }

            public Task OnRedirectToIdentityProvider(RedirectContext context, TokenProviderConfiguration configuration)
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
                if (IsMfaRequired(context, configuration))
                    context.ProtocolMessage.SetParameter("mfa_required", "true");
                return Task.CompletedTask;
            }

            private static bool IsMfaRequired(RedirectContext context, TokenProviderConfiguration configuration)
            {
                return configuration.RequireMfa || (_conditionalMfaFunc?.Invoke(context.HttpContext, context.Properties) ?? false);
            }

            private static bool IsMfaRequired(AuthorizationCodeReceivedContext context, TokenProviderConfiguration configuration)
            {
                return configuration.RequireMfa || (_conditionalMfaFunc?.Invoke(context.HttpContext, context.Properties) ?? false);
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

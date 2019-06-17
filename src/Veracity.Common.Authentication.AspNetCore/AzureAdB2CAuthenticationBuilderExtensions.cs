using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;
using Veracity.Common.OAuth;
using Microsoft.AspNetCore.Builder;

namespace Veracity.Common.Authentication.AspNetCore
{
    internal class NullConfig : IConfigurationReader
    {
        public NameValueCollection AppSettings { get; } = new NameValueCollection();
    }
    internal class LogWrapper : ILogger, ILogging
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LogWrapper(ILogger<VeracityService> logger)
        {
            _logger = logger;
        }
        public void Error(Exception error)
        {
            _logger?.LogError(error, error.Message);
        }

        public void Message(string message)
        {
            _logger?.LogDebug(message);
        }

        public void Message(string format, params object[] args)
        {
            Message(string.Format(format, args));
        }

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            Error(exceptionToLog);
        }

        public void HeartBeat()
        {

        }

        public void DebugMessage(string message, LogType entryType = LogType.Information, string additionalDebugInformation = null)
        {
            Message(message);
        }

        public void SetCommonProperties(string logName)
        {
        }
    }

    public abstract class VeracityService
    {
    }
    public static class VeracityIdExtensions
    {

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key, TokenProviderConfiguration tokenProviderConfiguration)
        {
            ConfigurationManagerHelper.SetManager(new NullConfig());
            configuration.Bind(key, tokenProviderConfiguration);
            return services;
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddVeracity(configuration, "Veracity", new TokenProviderConfiguration());
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key)
        {
            return services.AddVeracity(configuration, key, new TokenProviderConfiguration());
        }

        public static IServiceCollection AddVeracity(this IServiceCollection services, IConfiguration configuration, string key, out TokenProviderConfiguration tokenProviderConfiguration)
        {
            ConfigurationManagerHelper.SetManager(new NullConfig());

            var t = new TokenProviderConfiguration();
            configuration.Bind(key, t);
            tokenProviderConfiguration = t;
            return services;
        }

        /// <summary>
        /// Binds the veracity related configuration settings to aspnetcore
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVeracity<T>(this IServiceCollection services) where T : IConfigurationReader, new()
        {
            ConfigurationManagerHelper.SetManager(new T());
            return services;
        }
        /// <summary>
        /// Binds the veracity related configuration settings to aspnetcore
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>

        public static IServiceCollection AddVeracity(this IServiceCollection services, Func<IConfigurationReader> func)
        {
            ConfigurationManagerHelper.SetManager(func.Invoke());
            return services;
        }
        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, Action<AzureAdB2COptions> options)
        {

            builder.AddAzureAdB2C(options);
            return builder;
        }

        public static AuthenticationBuilder AddVeracityAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, Func<AuthorizationCodeReceivedContext, Task> additionalAuthCodeHandling = null)
        {
            AzureAdB2CAuthenticationBuilderExtensions.AdditionalAuthCodeHandling = additionalAuthCodeHandling;
            builder.AddVeracityAuthentication(options =>
            {
                configuration.Bind("AzureAdB2C", options);

            });
            return builder;
        }
        public static IApplicationBuilder UseVeracity(this IApplicationBuilder app)
        {

            //OauthAttribute.SetOauthProvider(new TokenProvider(app.ApplicationServices));
            return app;
        }

    }
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

            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>()
                .AddScoped<ITokenHandler, TokenProvider>()
                .AddSingleton<TokenProviderConfiguration, TokenProviderConfiguration>()
                .AddHttpContextAccessor()
                .AddSingleton<ILogger, LogWrapper>()
                .AddSingleton<ILogging, LogWrapper>()
                .AddScoped(s => s.GetService<IHttpContextAccessor>().HttpContext.User)

                .AddScoped<TokenCacheBase, DistributedTokenCache>();
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
                _logger?.Message("Auth code received...");
                try
                {
                    arg.HttpContext.User = arg.Principal;
                    var cache = arg.HttpContext.RequestServices.GetService<TokenCacheBase>();
                    var context = new ConfidentialClientApplication(ClientId(configuration), Authority(configuration), configuration.RedirectUrl, new ClientCredential(configuration.ClientSecret), cache, null);
                    var user = await context.AcquireTokenByAuthorizationCodeAsync(arg.ProtocolMessage.Code, new[] { configuration.Scope });
                    var policyValidator = arg.HttpContext.RequestServices.GetService<IPolicyValidation>();
                    try
                    {
                        if (policyValidator != null)
                        {
                            var policy = await ValidatePolicies(configuration, policyValidator, arg.ProtocolMessage.RedirectUri);
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

            public Task OnRedirectToIdentityProvider(RedirectContext context)
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

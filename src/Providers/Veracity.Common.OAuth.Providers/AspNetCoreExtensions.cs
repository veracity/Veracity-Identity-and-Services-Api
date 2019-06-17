using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Veracity.Common.Authentication;
using Veracity.Common.Authentication.AspNetCore;
using Veracity.Services.Api;
using ILogger = Veracity.Common.Authentication.ILogger;

#pragma warning disable 618

namespace Veracity.Common.OAuth.Providers
{


    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger _logger;

        public ErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        private readonly bool _overrideDefaults = true;

        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            if (exception != null) _logger?.Error(exception);
            return null;
        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            if (error != null) _logger?.Error(error);
            return null;
        }

        public bool OverrideDefaults => _overrideDefaults;
    }
    public static class AspNetCoreExtensions
    {

        /// <summary>
        /// Adds the veracity api services to the IOC container.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="myServicesApiBaseUrl">the base address for the service</param>
        /// <returns></returns>
        public static IServiceCollection AddVeracityServices(this IServiceCollection services, string myServicesApiBaseUrl)
        {
            services.AddInterstellar();
            services.AddScoped(s => s.CreateRestClient<IMy>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IUsersDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IThis>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<ICompaniesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IOptions>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IServicesDirectory>(myServicesApiBaseUrl));
            services.AddScoped(s => s.CreateRestClient<IDataContainerService>(myServicesApiBaseUrl));
            services.AddScoped(s => new ApiClientConfiguration(myServicesApiBaseUrl));
            services.AddScoped<IPolicyValidation, PolicyValidation>();
            services.AddSingleton<IDataProtector, DataProtectorNetCore>();
            //Or add a common api accessor for the veracity MyServices api.
            services.AddScoped<IApiClient, ApiClient>();
            return services;
        }


        /// <summary>
        /// Adds the veracity api as controllers for easy access from javascript
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static IMvcBuilder AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(this IMvcBuilder builder, string baseUrl, string authenticationSchemes) where TUserNameResolver : class, IUserNameResolver where TErrorHandler : class, IErrorHandler
        {
            builder.Services.AddScoped<Stardust.Interstellar.Rest.Common.ILogger,InternalLogger>();
            builder.Services
                .AddSingleton<IUserNameResolver, TUserNameResolver>()
                .AddSingleton<IErrorHandler, TErrorHandler>();
            
            ClientFactory.SetServiceProviderFactory(() => builder.Services.BuildServiceProvider());
            builder.Services.SetAuthenticationSchemes(authenticationSchemes);
            builder.AddAsController(s => s.CreateRestClient<IMy>(baseUrl))
                .AddAsController(s => s.CreateRestClient<IThis>(baseUrl))
                //.AddAsController(s => s.CreateRestClient<ICompaniesDirectory>(baseUrl))
                //.AddAsController(s => s.CreateRestClient<IServicesDirectory>(baseUrl))
                //.AddAsController(s => s.CreateRestClient<IUsersDirectory>(baseUrl))
                .UseInterstellar();
            return builder;
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder, string baseUrl,
            string authenticationSchemes)
        {
            return builder.AddVeracityApiProxies<ErrorHandler, UserNameResolver>(baseUrl, authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder, string authenticationSchemes)
        {
            return builder.AddVeracityApiProxies(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(this IMvcBuilder builder, string authenticationSchemes) where TUserNameResolver : class, IUserNameResolver where TErrorHandler : class, IErrorHandler
        {
            return builder.AddVeracityApiProxies<TErrorHandler, TUserNameResolver>(ConfigurationManagerHelper.GetValueOnKey("myApiV3Url"), authenticationSchemes);
        }

        public static IMvcBuilder AddVeracityApiProxies(this IMvcBuilder builder)
        {
            return builder.AddVeracityApiProxies("Cookies");
        }
    }

    public class InternalLogger : Stardust.Interstellar.Rest.Common.ILogger
    {
        private readonly ILogger _logger;

        public InternalLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Error(Exception error)
        {
            _logger?.Error(error);
        }

        public void Message(string message)
        {
            _logger?.Message(message);
        }

        public void Message(string format, params object[] args)
        {
            _logger?.Message(format,args);
        }
    }

    public class PolicyValidation : IPolicyValidation
    {
        private readonly IMy _myService;
        private readonly ILogger _logger;

        public PolicyValidation(IMy myService, ILogger logger)
        {
            _myService = myService;
            _logger = logger;
        }
        public async Task<ValidationResult> ValidatePolicy(string protocolMessageRedirectUri)
        {
            try
            {
                await _myService.ValidatePolicies(protocolMessageRedirectUri);
                return new ValidationResult
                {
                    AllPoliciesValid = true
                };
            }
            catch (ServerException e)
            {
                return HandleValidationResponse(e);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ServerException e)
                    return HandleValidationResponse(e);
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return new ValidationResult
            {
                AllPoliciesValid = true
            };
        }

        private ValidationResult HandleValidationResponse(ServerException e)
        {
            _logger.Error(e);
            if (e.Status == HttpStatusCode.NotAcceptable)
            {

                var url = e.GetErrorData<ValidationError>().Url; //Getting the redirect url from the error message.
                return new ValidationResult
                {
                    RedirectUrl = url,
                    AllPoliciesValid = false
                };
            }
            return new ValidationResult
            {
                AllPoliciesValid = true
            };
        }

        public async Task<ValidationResult> ValidatePolicyWithServiceSpesificTerms(string serviceId, string protocolMessageRedirectUri)
        {
            try
            {
                await _myService.ValidatePolicy(serviceId,protocolMessageRedirectUri);
                return new ValidationResult
                {
                    AllPoliciesValid = true
                };
            }
            catch (ServerException e)
            {
                return HandleValidationResponse(e);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ServerException e)
                    return HandleValidationResponse(e);
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return new ValidationResult
            {
                AllPoliciesValid = true
            };
        }
    }



}
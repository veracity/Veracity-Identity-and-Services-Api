﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Dependencyinjection;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Veracity.Common.Authentication;
using Veracity.Services.Api;
using Veracity.Services.Api.Extensions;

namespace Veracity.Common.OAuth.Providers
{
    public class ServicesConfig : ServicesConfiguration
    {
        public virtual bool IncludeProxies => false;

        protected override IServiceCollection Configure(IServiceCollection services)
        {
            services.AddScoped<ITokenHandler,TokenProvider>();
            if (IncludeProxies)
                AddProxies(services);
            services.AddScoped<IPolicyValidation, PolicyValidation>();
            services.AddVeracity(ConfigurationManager.AppSettings["myApiV3Url"]);
            return services;
        }

        public IServiceCollection AddProxies(IServiceCollection services, bool doFinalize = true)
        {// typeof(IHttpControllerTypeResolver), new CustomAssebliesResolver()

            return services.AddVeracityProxies(doFinalize);
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

        public PolicyValidation(IMy myService)
        {
            _myService = myService;
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
                _logger?.Error(ex);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
            }
            return new ValidationResult
            {
                AllPoliciesValid = true
            };
        }

        private ValidationResult HandleValidationResponse(ServerException e)
        {
            _logger?.Error(e);
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
                await _myService.ValidatePolicy(serviceId, protocolMessageRedirectUri);
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
                _logger?.Error(ex);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
            }
            return new ValidationResult
            {
                AllPoliciesValid = true
            };
        }
    }
}

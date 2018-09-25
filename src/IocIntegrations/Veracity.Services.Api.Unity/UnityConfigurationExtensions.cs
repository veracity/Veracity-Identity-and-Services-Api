﻿using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;
using System;
using System.Configuration;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Veracity.Services.Api.UnityAdapter
{
    public static class UnityConfigurationExtensions
    {
        public static IUnityContainer AddVeracity<TLogger>(this IUnityContainer container) where TLogger : ILogger
        {
            container.RegisterType<IProxyFactory>(new InjectionFactory(s =>
                new ProxyFactoryImplementation(new Locator(new UnityServiceLocator(s)))));
            container.RegisterType<IServiceProvider>(new InjectionFactory(s => new UnityServiceLocator(s)))
                .RegisterType<IApiClientConfiguration, ApiClientConfigurationHelper>()
                .RegisterType<IMy>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>()
                        .CreateRestClient<IMy>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IThis>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>()
                        .CreateRestClient<IThis>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IServicesDirectory>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>().CreateRestClient<IServicesDirectory>(
                        ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<ICompaniesDirectory>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>().CreateRestClient<ICompaniesDirectory>(
                        ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IUsersDirectory>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>().CreateRestClient<IUsersDirectory>(
                        ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IDataContainerService>(new InjectionFactory(s =>
                    s.Resolve<IServiceProvider>().CreateRestClient<IDataContainerService>(
                        ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IApiClient, ApiClient>()
                .RegisterType<ILogger, TLogger>();
            return container;
        }

        public static IUnityContainer AddVeracity(this IUnityContainer container)
        {
            return container.AddVeracity<NullLogger>();
        }
    }
}
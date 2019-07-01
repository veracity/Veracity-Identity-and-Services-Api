using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;
using System;
using System.Configuration;

namespace Veracity.Services.Api.AutofacAdapter
{
    public static class AutofacConfigurationExtensions
    {
        /// <summary>
        /// Registers all needed Veracity components with autofac
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="populateServiceCollection"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterVeracity(this ContainerBuilder builder, bool populateServiceCollection = true)
        {
            return builder.RegisterVeracity<NullLogger>(populateServiceCollection);
        }

        public static T CreateRestClient<T>(this IComponentContext context, string baseUrl)
        {
            return context.Resolve<IServiceProvider>().CreateRestClient<T>(baseUrl);
        }
        /// <summary>
        /// Registers all needed Veracity components with autofac
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="builder"></param>
        /// <param name="populateServiceCollection"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterVeracity<TLogger>(this ContainerBuilder builder, bool populateServiceCollection = true) where TLogger : ILogger
        {
            builder.Populate(new ServiceCollection());
            builder.RegisterType<Locator>().As<IServiceLocator>().InstancePerLifetimeScope();
            builder.RegisterType<ProxyFactoryImplementation>().As<IProxyFactory>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<IMy>(ConfigurationManager.AppSettings["myApiV3Url"])).As<IMy>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<ICompaniesDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])).As<ICompaniesDirectory>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<IThis>(ConfigurationManager.AppSettings["myApiV3Url"])).As<IThis>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<IServicesDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])).As<IServicesDirectory>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<IUsersDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])).As<IUsersDirectory>().InstancePerLifetimeScope();
            builder.Register((c, p) => c.CreateRestClient<IDataContainerService>(ConfigurationManager.AppSettings["myApiV3Url"])).As<IDataContainerService>().InstancePerLifetimeScope();
            builder.RegisterType<TLogger>().As<ILogger>().InstancePerLifetimeScope();
            builder.RegisterType<ApiClient>().As<IApiClient>().InstancePerLifetimeScope();
            builder.Register((c, p) => new ApiClientConfiguration()).As<IApiClientConfiguration>().SingleInstance();
            return builder;
        }
    }
}

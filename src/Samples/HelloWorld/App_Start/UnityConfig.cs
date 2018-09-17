using HelloWorld.Controllers;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Common;
using System;
using System.Configuration;
using Unity;
using Unity.Injection;
using Veracity.Services.Api;

namespace HelloWorld
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(UnityContainer container)
        {

            container.RegisterType<IProxyFactory>(new InjectionFactory(s => new ProxyFactoryImplementation(new Locator((IServiceProvider)s))))
                .RegisterType<IServiceProvider, UnityContainer>()
                .RegisterType<IApiClientConfiguration, ApiClientConfigurationHelper>()
                .RegisterType<IMy>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<IMy>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IThis>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<IThis>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IServicesDirectory>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<IServicesDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<ICompaniesDirectory>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<ICompaniesDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IUsersDirectory>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<IUsersDirectory>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IDataContainerService>(new InjectionFactory(s =>
                    (s as UnityContainer).CreateRestClient<IDataContainerService>(ConfigurationManager.AppSettings["myApiV3Url"])))
                .RegisterType<IApiClient, ApiClient>()
                .RegisterType<HomeController, HomeController>();
            // .RegisterType<IMy>(new InjectionFactory(s => (s as IServiceProvider).CreateRestClient<IMy>(ConfigurationManager.AppSettings["myApiV3Url"])));

            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();
            //container.RegisterTypeIServiceLocator,Locator).RegisterType<IProxyFactory,ProxyFactoryImplementation>();
            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
        }
    }

    public class ApiClientConfigurationHelper : ApiClientConfiguration
    {
        public ApiClientConfigurationHelper() : base()
        { }
    }

}

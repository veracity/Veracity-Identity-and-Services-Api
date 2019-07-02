#if NET471
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Service;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Veracity.Services.Api;

namespace Veracity.Common.OAuth.Providers
{
	public static class OwinExtensions
	{
        

		/// <summary>
		/// Call this to add the veracity services to the ioc
		/// </summary>
		/// <param name="services"></param>
		/// <param name="veracityApiBaseUrl"></param>
		/// <returns></returns>
		public static IServiceCollection AddVeracity(this IServiceCollection services, string veracityApiBaseUrl)
		{
			return services.AddInterstellarClient()
				.AddInterstellarClient<IMy>(veracityApiBaseUrl)
				.AddInterstellarClient<IThis>(veracityApiBaseUrl)
				.AddInterstellarClient<IServicesDirectory>(veracityApiBaseUrl)
				.AddInterstellarClient<IUsersDirectory>(veracityApiBaseUrl)
				.AddInterstellarClient<ICompaniesDirectory>(veracityApiBaseUrl)
				.AddSingleton<IApiClient, ApiClient>()
				.AddInterstellarClient<IDataContainerService>(veracityApiBaseUrl)
				.AddSingleton<IApiClientConfiguration>(s => new ApiClientConfiguration(veracityApiBaseUrl));
		}

		public static IServiceCollection AddVeracityProxies(this IServiceCollection services, bool doFinalize = true)
		{
			services.AddInterstellarServices();
			var locator = new Locator(services.BuildServiceProvider());
			services.AddScoped(ServiceFactory.CreateServiceImplementation<IMy>(locator));
			services.AddScoped(ServiceFactory.CreateServiceImplementation<IThis>(locator));
			if (doFinalize)
			{
				//GlobalConfiguration.Configuration.Services
				services.FinalizeRegistration()
					.AddSingleton<IHttpControllerTypeResolver>(s => new CustomAssebliesResolver());
				GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new WrapperResolver(new CustomAssebliesResolver()));
			}
			return services;
		}

		
	}
}
#endif
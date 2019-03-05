using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api
{
	[Api("")]
	[CircuitBreaker(100, 5)]
	[SupportCode]
	[ErrorHandler(typeof(ExceptionWrapper))]
	[ServiceInformation]
	public interface IOptions : IVeracityService
	{
		[Head("info")]
		Task GetViewPoints();

		[Options("options/{viewPoint}", "Provides CORS requirements and details about the viewpoints")]
		Task GetOptions1([In(InclutionTypes.Path)] string viewPoint);

		[Options("options")]
		Task GetOptions();

		[Get("Status", "Get the status of the service container.")]
		[AuthorizeWrapper]
		Task<Dictionary<string, string>> ServiceStatus();

		[Get("cache/invalidate/{id}", "Invalidate the cached item with the provided id")]
		[AuthorizeWrapper]
		Task InvalidateCache([In(InclutionTypes.Path)]string id);
	}
}
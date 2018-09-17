using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api
{
    [IRoutePrefix("")]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [ServiceInformation]
    public interface IOptions : IVeracityService
    {
        [Head]
        [IRoute("info")]
        Task GetViewPoints();

        [Options]
        [IRoute("options/{viewPoint}")]
        [ServiceDescription("Provides CORS requirements and details about the viewpoints")]
        Task GetOptions1([In(InclutionTypes.Path)] string viewPoint);

        [Options]
        [IRoute("options")]
        Task GetOptions();

        [Get]
        [IRoute("Status")]
        [AuthorizeWrapper()]
        [ServiceDescription("Get the status of the service container.")]
        [AuthorizeWrapper]
        Task<Dictionary<string, string>> ServiceStatus();

        [Get]
        [IRoute("cache/invalidate/{id}")]
        [AuthorizeWrapper()]
        [ServiceDescription("Invalidate the cached item with the provided id")]
        [AuthorizeWrapper]
        Task InvalidateCache([In(InclutionTypes.Path)]string id);
    }
}
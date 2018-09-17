using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [IRoutePrefix("directory")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IDataContainerService
    {

        [Get]
        [IRoute("services/{serviceId}/datacontainers")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Get associated data containers for the service")]
        Task<IEnumerable<ItemReference>> GetDataContainers([InPath] string serviceId);

        [Put]
        [IRoute("services/{serviceId}/datacontainers/{containerId}")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Create a data container reference")]
        Task CreateDataContainer([InPath] string serviceId, [InPath] string containerId, [InQuery]string name);

        [Delete]
        [IRoute("services/{serviceId}/datacontainers/{containerId}")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Delete a data container reference")]
        Task DeleteDataContainer([InPath] string serviceId, [InPath] string containerId);
    }
}

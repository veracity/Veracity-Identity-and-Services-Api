using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [Api("directory")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IDataContainerService
    {

        [Get("services/{serviceId}/datacontainers", "Get associated data containers for the service")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<ItemReference>> GetDataContainers([InPath] string serviceId);

        [Put("services/{serviceId}/datacontainers/{containerId}", "Create a data container reference")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task CreateDataContainer([InPath] string serviceId, [InPath] string containerId, [InQuery]string name);

        [Delete("services/{serviceId}/datacontainers/{containerId}", "Delete a data container reference")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task DeleteDataContainer([InPath] string serviceId, [InPath] string containerId);
    }
}

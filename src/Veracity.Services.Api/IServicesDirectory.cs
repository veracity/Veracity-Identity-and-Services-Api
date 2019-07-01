using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;
// ReSharper disable UnusedMember.Global

namespace Veracity.Services.Api
{
    [Api("directory")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IServicesDirectory : IVeracityService
    {
        [Get("services/{id}", "Get the detailed service description by the provided id")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<ServiceInfo> GetServiceById([In(InclutionTypes.Path)] string id);


        [Get("services/{id}/users", "Get the list of users subscribing to the service. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<UserReference>> GetUsers([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get("services/{serviceId}/administrators/{userId}","Internal only")]
        [Obsolete("Only for Veracity internal usage, this will be removed at a later stage", false)]
        Task<bool> IsAdmin([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Path)] string serviceId);
    }
}
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
    //[CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IServicesDirectory : IVeracityService
    {
        [Get("services/{id}", "Get the detailed service description by the provided id")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<ServiceInfo> GetServiceById([In(InclusionTypes.Path)] string id, [In(InclusionTypes.Header)] string tenantId = null);


        [Get("services/{id}/users", "Get the list of users subscribing to the service. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<UserReference>> GetUsers([In(InclusionTypes.Path)] string id, [In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize, [In(InclusionTypes.Header)] string tenantId = null);

        [Get("services/{serviceId}/administrators/{userId}","Internal only")]
        [Obsolete("Only for Veracity internal usage, this will be removed at a later stage", false)]
        Task<bool> IsAdmin([In(InclusionTypes.Path)] string userId, [In(InclusionTypes.Path)] string serviceId);
    }
}
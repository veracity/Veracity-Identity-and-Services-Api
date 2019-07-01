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
    public interface ICompaniesDirectory : IVeracityService
    {
        [Get("companies/{id}", "Get the detailed company description")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<CompanyInfo> CompanyById([In(InclutionTypes.Path)] string id);

        [Get("companies/{id}/users", "Get users affiliated with the company. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<UserReference>> GetUsersByCompany([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

    }
}
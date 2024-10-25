using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [Api("directory/users")]
    [Oauth]
    //[CircuitBreaker(1000, 2)]
    [SupportCode]
    [ErrorHandler(errorHandlerType: typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]

    public interface IUsersDirectory : IVeracityService
    {
        [Get("email/", "Gets a list of users with a given email address")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<UserReference>> GetUsersByEmail([In(InclutionTypes.Path)] string email);

        [Get("{id}", "Returns the full profile for the user with the provided id")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<UserInfo> GetUser([In(InclutionTypes.Path)] string id);

        [Post("", "Get full user profiles for a list of userid's", Summary = "Read multiple users profile")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<UserInfo[]> GetUsersIn([In(InclutionTypes.Body)] string[] ids);



        [Get("{userid}/companies", "Returns a list of companies tied to a spescified user.")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<CompanyReference>> GetUserCompanies([In(InclutionTypes.Path)] string userid);

        [Get("{userid}/services", "Get a list of the users servcies in the tenant if provided, or it will use system tenant. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<IEnumerable<ServiceReference>> GetUserServices([In(InclutionTypes.Path)] string userid, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize, [InHeader] string tenantId = null);

        [Get("{userid}/services/{serviceId}", "Gets the subscription state for a user with respect to the service provided in the tenant if provided, or it will use system tenant.")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<SubscriptionDetails> GetUserSubscriptionDetails([In(InclutionTypes.Path)] string userid, [In(InclutionTypes.Path)] string serviceId, [InHeader] string tenantId = null);


    }
}
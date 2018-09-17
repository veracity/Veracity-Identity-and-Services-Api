using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [IRoutePrefix("directory/users")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(errorHandlerType: typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]

    public interface IUsersDirectory : IVeracityService
    {
        [Get]
        [IRoute("email/")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Gets a list of users with a given email address")]
        Task<IEnumerable<UserReference>> GetUsersByEmail([In(InclutionTypes.Path)] string email);

        [Get]
        [IRoute("{id}")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Returns the full profile for the user with the provided id")]
        Task<UserInfo> GetUser([In(InclutionTypes.Path)] string id);

        [Post]
        [IRoute("")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Get full user profiles for a list of userid's", Summary = "Read multiple users profile")]
        Task<UserInfo[]> GetUsersIn([In(InclutionTypes.Body)] string[] ids);



        [Get]
        [IRoute("{userid}/companies")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Returns a list of companies tied to a spescified user.")]
        Task<IEnumerable<CompanyReference>> GetUserCompanies([In(InclutionTypes.Path)] string userid);

        [Get]
        [IRoute("{userid}/services")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        [ServiceDescription("Get a list of the users servcies. Paged query: uses 0 based page index")]
        Task<IEnumerable<ServiceReference>> GetUserServices([In(InclutionTypes.Path)] string userid, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get]
        [IRoute("{userid}/services/{serviceId}")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadDirectoryAccess)]
        Task<SubscriptionDetails> GetUserSubscriptionDetails([In(InclutionTypes.Path)] string userid, [In(InclutionTypes.Path)] string serviceId);


    }
}
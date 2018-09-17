using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [IRoutePrefix("this")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IThis : IVeracityService
    {
        [Get]
        [IRoute("services")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        [ServiceDescription("Get all services the service principal has access to. Currently not 100% accurate. Paged query: uses 0 based page index")]
        Task<IEnumerable<ServiceReference>> GetServices([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get]
        [IRoute("subscribers")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        [ServiceDescription("Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        Task<IEnumerable<UserReference>> GetUsers([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get]
        [IRoute("services/{serviceId}/subscribers")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        [ServiceDescription("Get all users with a subscription to this service. Paged query: uses 0 based page index")]
        Task<IEnumerable<UserReference>> GetServiceUsers([In(InclutionTypes.Path)]string serviceId, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Put]
        [IRoute("subscribers/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        [ServiceDescription("Add a user subscription to the service", Summary = Constants.Warning300)]
        Task AddUserAsync([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Body)] SubscriptionOptions options);

        [Put]
        [IRoute("services/{serviceId}/subscribers/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        [ServiceDescription("Add a user subscription to the service with the provided id .Only available for the root service for nested services")]
        Task AddServiceUser([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Body)] SubscriptionOptions options);

        [Delete]
        [IRoute("services/{serviceId}/subscribers/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        [ServiceDescription("Remove servive subscription from the user .Only available for the root service for nested services")]
        Task RemoveServiceUser([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Path)] string serviceId);

        [Delete]
        [IRoute("subscribers/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        [ServiceDescription("Remove servive subscription from the user ", Summary = Constants.Warning300)]
        Task RemoveUser([In(InclutionTypes.Path)] string userId);

        [Get]
        [IRoute("user/resolve({email})")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        [ServiceDescription("Get the user id from the email address", Summary = "Note that an email address may be connected to more than one user account")]
        Task<UserReference[]> ResolveUser([In(InclutionTypes.Path)] string email);

        [Post]
        [IRoute("user")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        [ServiceDescription("Create a user in myDNVGL")]
        Task<UserCreationReference> CreateUser([In(InclutionTypes.Body)] UserRegistration user);

        [Post]
        [IRoute("users")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        [ServiceDescription("Create a users in myDNVGL")]
        Task<UserCreationReference[]> CreateUsers([In(InclutionTypes.Body)] UserRegistration[] user);

        [Get]
        [IRoute("administrators")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        [ServiceDescription("Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        Task<IEnumerable<AdminReference>> GetAdmins([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get]
        [IRoute("administrators/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        [ServiceDescription("Get all users with a subscription to this service", Summary = Constants.Warning300)]
        Task<AdminInfo> GetAdmin([In(InclutionTypes.Path)] string userId);

        [Get]
        [IRoute("services/{serviceId}/administrators/{userId}")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        [ServiceDescription("Get all users with a subscription to this service")]
        Task<AdminInfo> GetServiceAdmin([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Path)] string userId);

        [Get]
        [IRoute("services/{serviceId}/administrators")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        [ServiceDescription("Get all users with a subscription to this service. Paged query: uses 0 based page index")]
        Task<IEnumerable<AdminReference>> GetServiceAdmins([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Post]
        [IRoute("services/{serviceId}/notification")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        [ServiceDescription("Send notification to your users through the Veracity notification service")]
        Task NotifyUsers([InPath] string serviceId, [In(InclutionTypes.Body)] NotificationMessage message, [InHeader] string channelId);


    }
}
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{
    [Api("this")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IThis : IVeracityService
    {
        [Get("services", "Get all services the service principal has access to. Currently not 100% accurate. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<ServiceReference>> GetServices([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get("subscribers", "Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<UserReference>> GetUsers([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get("subscribers/{userId}", "Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<UserReference> GetServiceUser([In(InclutionTypes.Path)] string userId);

        [Get("services/{serviceId}/subscribers/{userId}", "Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<UserReference> GetUserForService([InPath]string serviceId, [In(InclutionTypes.Path)] string userId);

        [Get("services/{serviceId}/subscribers", "Get all users with a subscription to this service. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<IEnumerable<UserReference>> GetServiceUsers([In(InclutionTypes.Path)]string serviceId, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Put("subscribers/{userId}", "Add a user subscription to the service", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task AddUserAsync([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Body)] SubscriptionOptions options);

        [Put("services/{serviceId}/subscribers/{userId}", "Add a user subscription to the service with the provided id .Only available for the root service for nested services")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        Task AddServiceUser([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Body)] SubscriptionOptions options);

        [Delete("services/{serviceId}/subscribers/{userId}", "Remove servive subscription from the user .Only available for the root service for nested services")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        Task RemoveServiceUser([In(InclutionTypes.Path)] string userId, [In(InclutionTypes.Path)] string serviceId);

        [Delete("subscribers/{userId}", "Remove servive subscription from the user ", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task RemoveUser([In(InclutionTypes.Path)] string userId);

        [Get("user/resolve({email})", "Get the user id from the email address", Summary = "Note that an email address may be connected to more than one user account")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        Task<UserReference[]> ResolveUser([In(InclutionTypes.Path)] string email);

        [Post("user", "Create a user in Veracity")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        Task<UserCreationReference> CreateUser([In(InclutionTypes.Body)] UserRegistration user);

        [Post("users", "Create users in Veracity")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task<UserCreationReference[]> CreateUsers([In(InclutionTypes.Body)] UserRegistration[] user);

        [Get("administrators", "Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<AdminReference>> GetAdmins([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get("administrators/{userId}", "Get all users with a subscription to this service", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<AdminInfo> GetAdmin([In(InclutionTypes.Path)] string userId);

        [Get("services/{serviceId}/administrators/{userId}", "Get all users with a subscription to this service")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<AdminInfo> GetServiceAdmin([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Path)] string userId);

        [Get("services/{serviceId}/administrators", "Get all users with a subscription to this service. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<IEnumerable<AdminReference>> GetServiceAdmins([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Post("services/{serviceId}/notification", "Send notification to your users through the Veracity notification service")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task NotifyUsers([InPath] string serviceId, [In(InclutionTypes.Body)] NotificationMessage message, [InHeader] string channelId);


    }
}
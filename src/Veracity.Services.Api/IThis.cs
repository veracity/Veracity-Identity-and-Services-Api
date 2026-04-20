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
    //[CircuitBreaker(1000, 2)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IThis : IVeracityService
    {
        [Get("services", "Get all services the service principal has access to. Currently not 100% accurate. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<ServiceReference>> GetServices([In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize);

        [Get("subscribers", "Get all users with a subscription to this servicein the tenant if provided, or it will get from system tenant. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<UserReference>> GetUsers([In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize, [InHeader] string tenantId = null);

        [Get("subscribers/{userId}", "Get all subscription to this user in the tenant if provided, or it will get from system tenant.", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<SubscriptionReference> GetServiceUser([In(InclusionTypes.Path)] string userId, [InHeader] string tenantId = null);

        [Get("services/{serviceId}/subscribers/{userId}", "Checks if a user has a subscription to the service and the access level if any in the tenant if provided, or it will get from system tenant.", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<SubscriptionReference> GetUserForService([InPath]string serviceId, [In(InclusionTypes.Path)] string userId, [InHeader] string tenantId = null);

        [Get("services/{serviceId}/subscribers", "Get all users with a subscription to this service in the tenant if provided, or it will get from system tenant. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<IEnumerable<UserReference>> GetServiceUsers([In(InclusionTypes.Path)]string serviceId, [In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize, [InHeader] string tenantId = null);

        [Put("subscribers/{userId}", "Add a user subscription to the service in the tenant if provided, or it will use system tenant", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task AddUserAsync([In(InclusionTypes.Path)] string userId, [In(InclusionTypes.Body)] SubscriptionOptions options, [InHeader] string tenantId = null);

        [Put("services/{serviceId}/subscribers/{userId}", "Add a user subscription to the service with the provided id in the tenant if provided, or it will use system tenant. Only available for the root service for nested services")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        Task AddServiceUser([In(InclusionTypes.Path)] string userId, [In(InclusionTypes.Path)] string serviceId, [In(InclusionTypes.Body)] SubscriptionOptions options, [InHeader] string tenantId = null);

        [Delete("services/{serviceId}/subscribers/{userId}", "Remove servive subscription from the user in the tenant if provided, or it will use system tenant. Only available for the root service for nested services")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions, ParameterIndex = 1)]
        Task RemoveServiceUser([In(InclusionTypes.Path)] string userId, [In(InclusionTypes.Path)] string serviceId, [InHeader] string tenantId = null);

        [Delete("subscribers/{userId}", "Remove servive subscription for the user from this service in the tenant if provided, or it will use system tenant.", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task RemoveUser([In(InclusionTypes.Path)] string userId, [InHeader] string tenantId = null);

        [Get("user/resolve({email})", "Get the user id from the email address", Summary = "Note that an email address may be connected to more than one user account")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        Task<UserReference[]> ResolveUser([In(InclusionTypes.Path)] string email);

        [Post("user", "Create a user in Veracity")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ManageServiceSubscriptions)]
        Task<UserCreationReference> CreateUser([In(InclusionTypes.Body)] UserRegistration user);

        [Post("users", "Create users in Veracity")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ManageServiceSubscriptions)]
        Task<UserCreationReference[]> CreateUsers([In(InclusionTypes.Body)] UserRegistration[] user);

        [Get("administrators", "Get all users with a subscription to this service. Paged query: uses 0 based page index", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<IEnumerable<AdminReference>> GetAdmins([In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize);

        [Get("administrators/{userId}", "Get all users with a subscription to this service", Summary = Constants.Warning300)]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess)]
        Task<AdminInfo> GetAdmin([In(InclusionTypes.Path)] string userId);

        [Get("services/{serviceId}/administrators/{userId}", "Get all users with a subscription to this service")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<AdminInfo> GetServiceAdmin([In(InclusionTypes.Path)] string serviceId, [In(InclusionTypes.Path)] string userId);

        [Get("services/{serviceId}/administrators", "Get all users with a subscription to this service. Paged query: uses 0 based page index")]
        [AccessControllGate(AccessControllTypes.UserAndService, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task<IEnumerable<AdminReference>> GetServiceAdmins([In(InclusionTypes.Path)] string serviceId, [In(InclusionTypes.Path)] int page, [In(InclusionTypes.Path)] int pageSize);

        [Post("services/{serviceId}/notification", "Send notification to your users through the Veracity notification service")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadAccess, ParameterIndex = 0)]
        Task NotifyUsers([InPath] string serviceId, [In(InclusionTypes.Body)] NotificationMessage message, [InHeader] string channelId);

        [Get("services/{serviceId}/subscribers/{userId}/picture", "Gets the profile picture of the user if there is a subscription and the user has uploaded a profile picture to veracity")]
        [AccessControllGate(AccessControllTypes.ServiceThenUser, RoleTypes.ReadAccess)]
        Task<ProfilePicture> GetUserProfilePicture([InPath] string serviceId, [In(InclusionTypes.Path)] string userId);

        [Get("services/{serviceId}/subscribers/{userId}/policy/validate()")]
        Task VerifySubscriberPolicy([In(InclusionTypes.Path)] string serviceId, [In(InclusionTypes.Path)] string userId, [In(InclusionTypes.Header)] string returnUrl, [InHeader] string skipSubscriptionCheck);
    }
}
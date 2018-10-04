using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;
using Veracity.Services.Api.Extensions;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{

    
    //Define the service as an interface almost as you would with a normal webapi controller. 
    //the IRoutePrefix translates into RoutePrefix attribute in webapi
    [IRoutePrefix("my")]
    [Oauth]
    [CircuitBreaker(100, 5)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
    public interface IMy : IVeracityService
    {
        [Get]
        [IRoute("profile")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        
        [ServiceDescription("Retreives the profile of the current loged in user.", Summary = "Note that we will remove the company node from the result in the future")]
        Task<MyUserInfo> Info();


        [Get]
        [IRoute("messages/count")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Get the current loged in users unread messages count")]
        Task<int> GetMessageCount();

        [Get]
        [IRoute("messages")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Read the users messages. All: include read messages")]
        Task<IEnumerable<MessageReference>> GetMessagesAsync([In(InclutionTypes.Path)] bool all);

        //Notice the In attribute for the parameters, it is equivalent with FromBody and FromUri and is required (it currently defaults to FromBody :(  )
        [Get]
        [IRoute("messages/{messageId}")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [Obsolete]
        Task<Message> GetMessageAsync([In(InclutionTypes.Path)] string messageId);

        [Patch]
        [IRoute("messages/{messageId}")]
        [Obsolete]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        Task MarkMessageAsRead();

        [Get]
        [IRoute("companies")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Get all companies related to the current user")]
        Task<List<CompanyReference>> GetMyCompanies();

        [Get]
        [IRoute("policies/{serviceId}/validate()")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Validates all myDnvgl policies and returns a list of the policies that needs attention")]
        [AuthorizeWrapper]
        Task ValidatePolicy([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Header)]string returnUrl);

        [Get]
        [IRoute("policies/validate()")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Validates all myDnvgl policies and returns a list of the policies that needs attention")]
        [AuthorizeWrapper]
        Task ValidatePolicies([In(InclutionTypes.Header)]string returnUrl);

        [Get]
        [IRoute("services")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [ServiceDescription("Returns all services for the user")]
        [AuthorizeWrapper]
        Task<IEnumerable<MyServiceReference>> MyServices();

        //[Put]
        //[IRoute("services/{serviceId}")]
        //[ServiceDescription("Add/apply for a service subscription")]
        //[AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        //[AuthorizeWrapper]
        //Task<ServiceInfo> SubscribeToService([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Body)] SubscriptionOptions options);

        //[Delete]
        //[IRoute("services/{serviceId}")]
        //[ServiceDescription("Remove the service form the users subscriptions")]
        //[AuthorizeWrapper]
        //Task RemoveService([In(InclutionTypes.Path)] string serviceId);
    }
}
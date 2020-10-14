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
    [Api("my")]
    [Oauth]
    [CircuitBreaker(1000, 2)]
    [SupportCode]
    [ErrorHandler(typeof(ExceptionWrapper))]
    [AuthorizeWrapper]
    [ServiceInformation]
    [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
    public interface IMy : IVeracityService
    {
        [Get("profile", "Retreives the profile of the current loged in user.", Summary = "Note that we will remove the company node from the result in the future")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        
        Task<MyUserInfo> Info();

        [Get("messages/count", "Get the current loged in users unread messages count")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        Task<int> GetMessageCount();

        [Get("messages", "Read the users messages. All: include read messages")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
       Task<IEnumerable<MessageReference>> GetMessagesAsync([In(InclutionTypes.Path)] bool all);

        //Notice the In attribute for the parameters, it is equivalent with FromBody and FromUri and is required (it currently defaults to FromBody :(  )
        [Get("messages/{messageId}")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [Obsolete]
        Task<Message> GetMessageAsync([In(InclutionTypes.Path)] string messageId);

        [Patch("messages/{messageId}")]
        [Obsolete]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        Task MarkMessageAsRead();

        [Get("companies", "Get all companies related to the current user")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        Task<List<CompanyReference>> GetMyCompanies();

        [Get("policies/{serviceId}/validate()", "Validates all myDnvgl policies and returns a list of the policies that needs attention")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [AuthorizeWrapper]
        Task ValidatePolicy([In(InclutionTypes.Path)] string serviceId, [In(InclutionTypes.Header)]string returnUrl,  [InHeader]string skipSubscriptionCheck);

        [Get("policies/validate()", "Validates all myDnvgl policies and returns a list of the policies that needs attention")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [AuthorizeWrapper]
        Task ValidatePolicies([In(InclutionTypes.Header)]string returnUrl);

        [Get("services", "Returns all services for the user")]
        [AccessControllGate(AccessControllTypes.User, RoleTypes.IsValidUser)]
        [AuthorizeWrapper]
        Task<IEnumerable<MyServiceReference>> MyServices();
    }
}
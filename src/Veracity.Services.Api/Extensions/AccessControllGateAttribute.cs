using System;
using System.Net;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Extensions;

namespace Veracity.Services.Api.Extensions
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class AccessControllGateAttribute : InputInterceptorAttribute
    {

        public AccessControllTypes AccessControllType { get; set; }

        public string RoleName { get; set; }

        public int ParameterIndex { get; set; } = -1;

        public AccessControllGateAttribute(AccessControllTypes accessControllType) : this(accessControllType, RoleTypes.ManageServiceSubscriptions)
        { }
        public AccessControllGateAttribute(AccessControllTypes accessControllType, string roleName)
        {
            AccessControllType = accessControllType;
            RoleName = roleName;

        }

        public override IInputInterceptor GetInterceptor(IServiceLocator serviceLocator)
        {
            var r = interceptorHandler?.Invoke(AccessControllType, RoleName, ParameterIndex, serviceLocator) ?? new NullInterceptor();
            return r;
        }

        private static Func<AccessControllTypes, string, int?, IServiceProvider, IInputInterceptor> interceptorHandler;

        public static void SetHandler(Func<AccessControllTypes, string, int?, IServiceProvider, IInputInterceptor> creator)
        {
            interceptorHandler = creator;
        }

    }

    public class NullInterceptor : IInputInterceptor
    {
        public object Intercept(object result, StateDictionary getState)
        {
            return result;
        }

        public bool Intercept(object[] values, StateDictionary stateDictionary, out string cancellationMessage,
            out HttpStatusCode statusCode)
        {
            cancellationMessage = null;
            statusCode = HttpStatusCode.OK;
            return false;
        }

        public Task<object> InterceptAsync(object result, StateDictionary getState)
        {
            return Task.FromResult(result);
        }

        public Task<InterseptorStatus> InterceptAsync(object[] values, StateDictionary stateDictionary)
        {
            return Task.FromResult(new InterseptorStatus { Cancel = false });
        }
    }
}
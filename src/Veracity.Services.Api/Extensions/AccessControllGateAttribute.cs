using System;
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
}
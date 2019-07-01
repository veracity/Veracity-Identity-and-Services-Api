using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;
using System;

namespace Veracity.Services.Api.Extensions
{
    public class ServiceInformationAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers(IServiceProvider serviceLocator)
        {
            var r = new IHeaderHandler[] { new ServiceInformationHandler(serviceLocator) };
            return r;
        }
    }

    
}

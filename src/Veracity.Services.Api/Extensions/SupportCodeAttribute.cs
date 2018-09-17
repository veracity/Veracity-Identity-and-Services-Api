using System;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Extensions;

namespace Veracity.Services.Api.Extensions
{
    public class SupportCodeAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers(IServiceProvider serviceLocator)
        {
            var r = new[] { new SupportCodeHandler(null) };
            return r;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Stardust.Interstellar.Rest.Service;

namespace Veracity.Common.OAuth.Providers
{
    public class WrapperResolver : IHttpControllerTypeResolver
    {
        private readonly CustomAssebliesResolver _customAssebliesResolver;

        public WrapperResolver(CustomAssebliesResolver customAssebliesResolver)
        {
            _customAssebliesResolver = customAssebliesResolver;
        }

        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {

            return _customAssebliesResolver.GetControllerTypes(assembliesResolver);
        }
    }
}
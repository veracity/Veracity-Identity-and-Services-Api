using System;
using Unity;

namespace Veracity.Services.Api.UnityAdapter
{
    public class UnityServiceLocator : IServiceProvider
    {
        private readonly IUnityContainer _container;

        public UnityServiceLocator(IUnityContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
                return _container.Resolve(serviceType);
            return null;
        }
    }
}
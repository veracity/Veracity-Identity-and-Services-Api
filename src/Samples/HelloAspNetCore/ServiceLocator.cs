using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Interstellar.Rest.Extensions;

namespace HelloAspNetCore
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            return _serviceProvider.GetServices<T>();
        }

        public object CreateInstanceOf(Type type)
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }

        public T CreateInstance<T>() where T : class
        {
            return CreateInstanceOf(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
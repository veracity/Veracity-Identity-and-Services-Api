using Stardust.Interstellar.Rest.Client;
using System;

namespace Veracity.Services.Api
{
    public class ApiClient : IApiClient
    {
        private readonly IMy _myService;
        private readonly IThis _thisService;
        private readonly IUsersDirectory _usersService;
        private readonly ICompaniesDirectory _companiesService;
        private readonly IServicesDirectory _servicesApi;
        private readonly IServiceProvider _serviceLocator;
        private readonly string _baseUrl;

        public ApiClient(IMy myService, IThis thisService, IUsersDirectory usersService, ICompaniesDirectory companiesService, IServicesDirectory servicesApi, IApiClientConfiguration configuration, IDataContainerService dataContainerService, IServiceProvider serviceLocator)
        {
            _myService = myService;
            _thisService = thisService;
            _usersService = usersService;
            _companiesService = companiesService;
            _servicesApi = servicesApi;
            _serviceLocator = serviceLocator;
            _baseUrl = configuration?.ApiBaseUrl;
            Directory = new Directory(_usersService, _companiesService, _servicesApi, dataContainerService, _serviceLocator);
        }

        public ApiClient(IMy myService, IThis thisService, IUsersDirectory usersService, ICompaniesDirectory companiesService, IServicesDirectory servicesApi, IDataContainerService dataContainerService, IServiceProvider serviceLocator)
        {
            _myService = myService;
            _thisService = thisService;
            _usersService = usersService;
            _companiesService = companiesService;
            _servicesApi = servicesApi;
            _serviceLocator = serviceLocator;
            Directory = new Directory(_usersService, _companiesService, _servicesApi, dataContainerService, _serviceLocator);
        }
        internal ApiClient(string baseUrl, IServiceProvider serviceLocator)
        {
            _baseUrl = baseUrl;
            _serviceLocator = serviceLocator;
            Directory = new Directory(_baseUrl);
        }

        public IMy My => _myService ?? _serviceLocator.CreateRestClient<IMy>(_baseUrl);
        public IThis This => _thisService ?? _serviceLocator.CreateRestClient<IThis>(_baseUrl);

        public IDirectory Directory { get; }
    }
}
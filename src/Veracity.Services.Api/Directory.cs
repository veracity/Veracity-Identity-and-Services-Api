using System;
using Stardust.Interstellar.Rest.Client;

namespace Veracity.Services.Api
{
    internal class Directory : IDirectory
    {
        private readonly IUsersDirectory _usersService;
        private readonly ICompaniesDirectory _companiesService;
        private readonly IServicesDirectory _servicesApi;
        private readonly IDataContainerService _dataContainerService;
        private readonly IServiceProvider _provider;
        private readonly string _baseUrl;

        internal Directory(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        internal Directory(IUsersDirectory usersService, ICompaniesDirectory companiesService, IServicesDirectory servicesApi,IDataContainerService dataContainerService, IServiceProvider provider)
        {
            _usersService = usersService;
            _companiesService = companiesService;
            _servicesApi = servicesApi;
            _dataContainerService = dataContainerService;
            _provider = provider;
        }

        public IUsersDirectory Users => _usersService ?? _provider.CreateRestClient<IUsersDirectory>(_baseUrl);
        public IDataContainerService DataContainer => _dataContainerService ?? _provider.CreateRestClient<IDataContainerService>(_baseUrl);

        public ICompaniesDirectory Companies => _companiesService ?? _provider.CreateRestClient<ICompaniesDirectory>(_baseUrl);

        public IServicesDirectory Services => _servicesApi ?? _provider.CreateRestClient<IServicesDirectory>(_baseUrl);
    }
}
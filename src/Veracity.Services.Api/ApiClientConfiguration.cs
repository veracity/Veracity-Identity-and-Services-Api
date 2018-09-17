using Stardust.Particles;

namespace Veracity.Services.Api
{
    public class ApiClientConfiguration : IApiClientConfiguration
    {
        public ApiClientConfiguration()
        {

        }

        public ApiClientConfiguration(string baseUrl)
        {
            _baseUrl = baseUrl;
        }
        private readonly string _baseUrl;
        public string ApiBaseUrl => _baseUrl ?? ConfigurationManagerHelper.GetValueOnKey("myApiV3Url");
    }
}
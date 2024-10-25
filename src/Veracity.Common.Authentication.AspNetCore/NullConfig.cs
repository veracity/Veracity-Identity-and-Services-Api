using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    internal class NullConfig : IConfigurationReader
    {
        private readonly IConfiguration _configuration;
        private readonly string _configSectionKey;

        public NullConfig(IConfiguration configuration, string configSectionKey)
        {
            _configuration = configuration;
            _configSectionKey = configSectionKey;
            var section = _configuration.GetSection(_configSectionKey);
            var reloadToke = section.GetReloadToken();

            foreach (var param in section.GetChildren())
            {
                AppSettings.Add(param.Key, param.Value);
            }
        }

        private void Callback(object obj)
        {
            AppSettings.Clear();
        }

        public NameValueCollection AppSettings { get; } = new NameValueCollection();
    }
}
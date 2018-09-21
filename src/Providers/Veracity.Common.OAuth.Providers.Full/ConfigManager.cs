using System.Collections.Specialized;
using System.Configuration;
using Stardust.Particles;

namespace Veracity.Common.OAuth.Providers
{
    public class ConfigManager : IConfigurationReader
    {
        public NameValueCollection AppSettings { get { return ConfigurationManager.AppSettings; } }
    }
}
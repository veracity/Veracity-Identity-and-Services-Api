using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Owin;
using Veracity.Common.OAuth;

namespace Veracity.Common.Authentication.AspNet
{
    public class ConfigurationWrapper
    {
        private readonly IAppBuilder _app;
        private readonly Dictionary<string, JObject> _config;

        public ConfigurationWrapper(IAppBuilder app, Dictionary<string, JObject> config)
        {
            _app = app;
            _config = config;
        }

        public IAppBuilder Environment(string environment)
        {
            JObject config;
            if (_config.TryGetValue(environment, out config))
            {
                var c = config.ToObject<TokenProviderConfiguration>();
            }
            return _app;
        }
    }
}
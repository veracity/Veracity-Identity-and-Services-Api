using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    public class ConfigManager : IConfigurationReader
    {
        public ConfigManager()
        {
            AppSettings = ConfigurationManager.AppSettings;
        }

        public ConfigManager(NameValueCollection additionalStore)
        {
            AppSettings=new NameValueCollection(ConfigurationManager.AppSettings);
            AppSettings.Add(additionalStore);
        }

        public NameValueCollection AppSettings { get; }
    
    }
    
}
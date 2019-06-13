using System;
using System.Collections.Specialized;
using System.IO;
using Microsoft.Extensions.Configuration;
using Stardust.Particles;

namespace Veracity.Common.Authentication.AspNetCore
{
    //public class ConfigWrapper : IConfigurationReader
    //{
    //    public ConfigWrapper()
    //    {
    //        var builder = new ConfigurationBuilder()
    //            .SetBasePath(Directory.GetCurrentDirectory())
    //            .AddJsonFile("appsettings.json");
    //        var root = builder.Build();
    //        AppSettings = new NameValueCollection();
    //        foreach (var item in root.AsEnumerable().Where(s => s.Key.StartsWith("Veracity", StringComparison.InvariantCultureIgnoreCase)))
    //        {
    //            AppSettings.Add(item.Key.Replace("Veracity:", ""), item.Value);
    //        }
    //    }
    //    public NameValueCollection AppSettings { get; }
    //}
}
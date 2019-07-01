using System.Collections.Specialized;
using Stardust.Particles;

namespace Veracity.Common.Authentication
{
    internal class NullConfig : IConfigurationReader
    {
        public NameValueCollection AppSettings { get; } = new NameValueCollection();
    }
}

using System;
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth.Providers
{
    public class InternalLogger : Stardust.Interstellar.Rest.Common.ILogger
    {
        private readonly ILogger _logger;

        public InternalLogger(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger)) as ILogger;
        }

        public void Error(Exception error)
        {
            _logger?.Error(error);
        }

        public void Message(string message)
        {
            _logger?.Message(message);
        }

        public void Message(string format, params object[] args)
        {
            _logger?.Message(format,args);
        }
    }
}
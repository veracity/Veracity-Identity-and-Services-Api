
using System;
using Microsoft.Extensions.Logging;
using Veracity.Common.Authentication;
using ILogger = Veracity.Common.Authentication.ILogger;

namespace Veracity.Common.OAuth.Providers
{
    public class InternalLogger : Stardust.Interstellar.Rest.Common.ILogger
    {
        private readonly ILogger _logger;

        public InternalLogger(IServiceProvider serviceProvider)
        {
            try
            {
                _logger = serviceProvider.GetService(typeof(ILogger<VeracityService>)) as ILogger;
            }
            catch (Exception)
            {

            }
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
using System;
using Microsoft.Extensions.Logging;
using Stardust.Particles;
using ILogger = Veracity.Common.Authentication.ILogger;

namespace HelloAspNetCore
{
    public class LogWrapper : ILogger, ILogging
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LogWrapper(ILogger<Startup> logger)
        {
            _logger = logger;
        }
        public void Error(Exception error)
        {
            _logger.LogError(error, error.Message);
        }

        public void Message(string message)
        {
            _logger.LogDebug(message);
        }

        public void Message(string format, params object[] args)
        {
            Message(string.Format(format, args));
        }

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            Error(exceptionToLog);
        }

        public void HeartBeat()
        {

        }

        public void DebugMessage(string message, LogType entryType = LogType.Information, string additionalDebugInformation = null)
        {
            Message(message);
        }

        public void SetCommonProperties(string logName)
        {
        }
    }
}
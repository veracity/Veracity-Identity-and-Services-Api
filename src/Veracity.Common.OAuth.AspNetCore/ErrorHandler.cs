
using System;
using System.Net;
using System.Net.Http;
using Stardust.Interstellar.Rest.Extensions;
using Veracity.Common.Authentication;

namespace Veracity.Common.OAuth.Providers
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger _logger;

        public ErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        private readonly bool _overrideDefaults = true;

        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            if (exception != null) _logger?.Error(exception);
            return null;
        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            if (error != null) _logger?.Error(error);
            return null;
        }

        public bool OverrideDefaults => _overrideDefaults;
    }
}
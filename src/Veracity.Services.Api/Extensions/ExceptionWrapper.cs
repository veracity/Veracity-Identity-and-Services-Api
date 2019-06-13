using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Stardust.Interstellar.Rest.Extensions;
using Stardust.Interstellar.Rest.Service;
using Stardust.Particles;
using Veracity.Common.Authentication;
using Veracity.Services.Api.Models;
using ILogger = Stardust.Interstellar.Rest.Common.ILogger;

namespace Veracity.Services.Api.Extensions
{


    public class ExceptionWrapper : IErrorHandler
    {
        private readonly IServiceLocator _locator;

        public ExceptionWrapper(IServiceLocator locator)
        {
            _locator = locator;
        }
        public HttpResponseMessage ConvertToErrorResponse(Exception exception, HttpRequestMessage request)
        {
            Logging.Exception(exception, request?.RequestUri?.ToString() ?? "WTF?!?");
            _locator?.GetService<IDiagnostics>()?.AddErrorTraceOuter(exception);
            if (exception?.InnerException is OperationAbortedException || exception?.InnerException is UnauthorizedAccessException)
            {

                var message = request.CreateResponse(HttpStatusCode.Forbidden, new ErrorDetail
                {
                    SubCode = SubCodes.Unauthorized,
                    Message = exception.InnerException.Message,
                    Information = "Authorization context is invalid",
                    SupportCode = GetSupportCode(request)
                });
                message.Headers.Add("x-sub-status", SubCodes.Unauthorized.ToString());
                return message;
            }
            var serverException = exception as ServerException;
            if (string.IsNullOrWhiteSpace(serverException?.ErrorObject?.SupportCode))
            {
                if (serverException != null && serverException.ErrorObject != null)
                    serverException.ErrorObject.SupportCode =
                        request?.Headers.GetValues("x-supportCode")?.FirstOrDefault();
            }
            var error = serverException != null ? CreateErrorMessage(request, serverException) : TranslateCommonExceptions(exception, request);
            if (serverException == null)
            {
                if (exception is OperationAbortedException abortException)
                {
                    error = request.CreateResponse(abortException.StatusCode, new ErrorDetail
                    {
                        Information = "request aborted",
                        Message = abortException.Message,
                        SubCode = 665,
                        SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
                    });
                }
            }
            if (error != null)
            {
                var errorDetail = serverException?.ErrorObject;
                if (errorDetail != null)
                {
                    errorDetail.SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault();
                    error.Headers.Add("x-sub-status", errorDetail.SubCode.ToString());
                }
            }

            return error;
        }

        private static HttpResponseMessage CreateErrorMessage(HttpRequestMessage request, ServerException serverException)
        {
            if (serverException.ErrorObject != null)
                return request.CreateResponse(serverException.Status, serverException.ErrorObject, serverException.Message);
            return request.CreateResponse(serverException.Status, serverException.ErrorData, serverException.Message);
        }

        private static string GetSupportCode(HttpRequestMessage request)
        {
            if (!request.Headers.TryGetValues("x-supportCode", out var supportCodes))
                supportCodes = new List<string>();
            return supportCodes.FirstOrDefault();
        }

        private HttpResponseMessage TranslateCommonExceptions(Exception exception, HttpRequestMessage request)
        {
            var error = (exception as AggregateException)?.InnerException ?? exception;
            if (error is NullReferenceException) return request.CreateResponse(HttpStatusCode.NotFound, new ErrorDetail
            {
                SubCode = SubCodes.EntityNotFound,
                Message = error.Message,
                Information = "Unable to locate resource or dependent resource",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is IndexOutOfRangeException) return request.CreateResponse(HttpStatusCode.Ambiguous, new ErrorDetail
            {
                SubCode = SubCodes.IndexOutOfRange,
                Message = error.Message,
                Information = "Unable to locate resource or dependent resource",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is InvalidCastException) return request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorDetail
            {
                SubCode = SubCodes.ValidationError,
                Message = error.Message,
                Information = "Type validation error",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is ArgumentNullException) return request.CreateResponse(HttpStatusCode.BadRequest, new ErrorDetail
            {
                SubCode = SubCodes.ValidationError,
                Message = error.Message,
                Information = "Missing input",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is ArgumentException) return request.CreateResponse(HttpStatusCode.BadRequest, new ErrorDetail
            {
                SubCode = SubCodes.ValidationError,
                Message = error.Message,
                Information = "Invalid input",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is UnauthorizedAccessException) return request.CreateResponse(HttpStatusCode.Forbidden, new ErrorDetail
            {
                SubCode = SubCodes.Unauthorized,
                Message = error.Message,
                Information = "Insufficient rights",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is HttpRequestException) return request.CreateResponse(HttpStatusCode.BadGateway, new ErrorDetail
            {
                SubCode = SubCodes.DependencyError,
                Message = error.Message,
                Information = "Dependency call failure",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            if (error is NotImplementedException) return request.CreateResponse(HttpStatusCode.NotImplemented, new ErrorDetail
            {
                SubCode = SubCodes.NotImplemented,
                Message = "Not implemented",
                Information = "Not supported in current release",
                SupportCode = request?.Headers.GetValues("x-supportCode")?.FirstOrDefault()
            });
            _locator?.GetService<ILogger>()?.Message($"Error of type {exception.GetType()} has occured");
            return null;

        }

        public Exception ProduceClientException(string statusMessage, HttpStatusCode status, Exception error, string value)
        {
            if (error is ServerException) return error;
            return new ServerException(status, statusMessage, error?.Message) { ErrorData = value };
        }

        public bool OverrideDefaults => true;
    }
}
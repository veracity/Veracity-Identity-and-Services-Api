using System;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Veracity.Services.Api.Models;

namespace Veracity.Services.Api
{

    public class ServerException : Exception
    {
        public HttpStatusCode Status { get; set; }
        public string ErrorData { get; set; }

        public ServerException(ErrorDetail errorData, HttpStatusCode status):base(errorData.Message)
        {
            Status = status;
            ErrorObject = errorData;
        }

        public ServerException(HttpStatusCode status, string errorBody, string message) : base(message)
        {
            Status = status;
            ErrorData = errorBody;
        }
        public ServerException(ErrorDetail errorData, HttpStatusCode status, Exception innerException)
            : base(errorData.Message, innerException)
        {
            Status = status;
            ErrorObject = errorData;
        }
        internal ErrorDetail ErrorObject { get; set; }

        public ServerException(ErrorDetail errorData, string message) : base(message)
        {
            ErrorObject = errorData;
        }

        public ServerException(ErrorDetail errorData, string message, Exception innerException) : base(message, innerException)
        {
            ErrorObject = errorData;
        }

        public T GetErrorData<T>() where T : ErrorDetail
        {
            if (string.IsNullOrWhiteSpace(ErrorData))
            {
                try
                {
                    var o = ErrorObject as T;
                    return o;
                }
                catch
                {
                    return default(T);

                }
            }
            return (T)JsonConvert.DeserializeObject(ErrorData, typeof(T));
        }

        protected ServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

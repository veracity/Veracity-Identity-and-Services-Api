using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Veracity.Services.Api.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode code, object errorMessage, string message = null)
        {
            if (errorMessage != null)
            {
                if (errorMessage is string)
                    return new HttpResponseMessage(code) { Content = new StringContent(errorMessage.ToString()), ReasonPhrase = message };
                return new HttpResponseMessage(code) { Content = new StringContent(JsonConvert.SerializeObject(errorMessage)), ReasonPhrase = message };
            }
            return new HttpResponseMessage(code) { ReasonPhrase = message };
        }
    }
}
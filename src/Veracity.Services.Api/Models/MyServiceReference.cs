using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class MyServiceReference : ServiceReference
    {
        /// <summary>
        /// the location of the application.
        /// </summary>
        [JsonProperty("serviceUrl")]
        public  string ServiceUrl { get; set; }
    }
}
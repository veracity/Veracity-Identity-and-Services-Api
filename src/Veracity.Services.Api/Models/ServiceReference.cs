using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class ServiceReference:ItemReference
    {
        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        public override string Identity => $"{MyExtensions.ServiceRootModifier}/services/{Id}";
    }

    public class MyServiceReference : ServiceReference
    {
        /// <summary>
        /// the location of the application.
        /// </summary>
        [JsonProperty("serviceUrl")]
        public  string ServiceUrl { get; set; }
    }
}
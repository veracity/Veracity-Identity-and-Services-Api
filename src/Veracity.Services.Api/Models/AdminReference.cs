using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class AdminReference : UserReference
    {
        [JsonProperty("accessLevelUrl")]
        public  string AccessLevelUrl => string.IsNullOrWhiteSpace(ServiceId) ?$"/this/administrators/{Id}": $"/this/administrators/{ServiceId}/{Id}";

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }
    }
}
using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class DataContainerReference : ItemReference
    {
        [JsonIgnore]
        public override string Identity => null;

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty("containerId")]
        public string ContainerId { get; set; }
    }
}
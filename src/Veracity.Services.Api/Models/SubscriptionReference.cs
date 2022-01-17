using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class SubscriptionReference : UserReference
    {
        public SubscriptionReference()
        {

        }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
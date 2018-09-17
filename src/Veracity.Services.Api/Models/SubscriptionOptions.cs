using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class SubscriptionOptions
    {
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
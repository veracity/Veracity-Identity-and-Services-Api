using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    public class SubscriptionState
    {
        [JsonProperty("state")]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))]
        public SubscriptionStateTypes State { get; set; }

        //Include when the underlying api supports it!
        //public DateTime LastChanged { get; set; }
    }
}
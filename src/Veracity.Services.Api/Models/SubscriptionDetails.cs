using System;
using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    public class SubscriptionDetails
    {
        [JsonProperty("service")]
        public ServiceReference Service { get; set; }

        [JsonProperty("user")]
        public UserReference User { get; set; }

        [JsonProperty("role")]
        public RoleReference Role { get; set; }

        [JsonProperty("subscriptionState")]
        public SubscriptionState SubscriptionState { get; set; }
    }

    public class SubscriptionState
    {
        [JsonProperty("state")]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))]
        public SubscriptionStateTypes State { get; set; }

        //Include when the underlying api supports it!
        //public DateTime LastChanged { get; set; }
    }
}
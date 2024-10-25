using System;
using Newtonsoft.Json;

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
        [JsonProperty("tenantId")]
        public Guid? TenantId { get; set; }

        //
        // Summary:
        //     The pricing tier of the service installed in the tenant, optional.
        [JsonProperty("pricingTier")]
        public string PricingTier { get; set; }
    }
}
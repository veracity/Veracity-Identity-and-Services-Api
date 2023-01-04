using Newtonsoft.Json;

namespace Veracity.Common.Authentication
{
    public class ValidationError : ErrorDetail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("violatedPolicies")]
        public string[] ViolatedPolicies { get; set; }

        [JsonProperty("subscriptionMissing")]
        public bool? SubscriptionMissing { get; set; }
    }
}
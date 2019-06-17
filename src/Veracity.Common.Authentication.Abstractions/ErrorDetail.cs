using Newtonsoft.Json;

namespace Veracity.Common.Authentication
{
    public class ErrorDetail
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("information")]
        public string Information { get; set; }

        [JsonProperty("subCode")]
        public int SubCode { get; set; }

        [JsonProperty("supportCode")]
        public string SupportCode { get; set; }
    }
}
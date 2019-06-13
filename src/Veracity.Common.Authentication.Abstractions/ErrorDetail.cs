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

    public class ValidationError : ErrorDetail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("violatedPolicies")]
        public string[] ViolatedPolicies { get; set; }
    }

    public class SubCodes
    {
        public static int EntityNotFound = 1;
        public static int IndexOutOfRange = 2;
        public static int ValidationError = 10;
        public static int Unauthorized = 100;
        public static int DependencyError = 3;
        public static int NotImplemented = 404;
        public static int DependencySuspended = 502;
        public static int Unknown = 4;
        public static int MissingPrincipal = 101;
        public static int InvalidPayload = 8;
    }
}
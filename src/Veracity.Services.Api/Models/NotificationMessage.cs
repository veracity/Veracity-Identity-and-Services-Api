using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class NotificationMessage
    {
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("recipients")]
        public string[] Recipients { get; set; }

        public bool HighPriority { get; set; }
    }
}
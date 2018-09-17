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

    public class Message
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timeStamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }
}
using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// A detailed role description of the availabe roles in myDNVGL
    /// </summary>
    public class Role
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("roleTypeId")]
        public int RoleTypeId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
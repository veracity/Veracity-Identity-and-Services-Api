using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public abstract class ItemReference
    {
        private string _id;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id
        {
            get => _id?.ToLowerInvariant();
            set => _id = value?.ToLowerInvariant();
        }

        [JsonProperty("description")]
        public virtual string Description { get; set; }

        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        [JsonProperty("identity", DefaultValueHandling = DefaultValueHandling.Include)]
        public abstract string Identity { get;  }
        
    }
}
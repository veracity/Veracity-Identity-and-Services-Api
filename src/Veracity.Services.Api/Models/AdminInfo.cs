using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class AdminInfo : UserInfo
    {
        /// <summary>
        /// Contains the administrator roles a user has for a given resource.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        [JsonProperty("roles")]
        public RoleReference[] Roles { get; set; }
    }
}
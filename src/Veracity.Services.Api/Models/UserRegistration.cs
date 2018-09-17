using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class UserRegistration
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Specify additional creation controll oprions, this is not mandatory 
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        [JsonProperty("options")]
        public RegistrationOptions Options { get; set; }

    }
}
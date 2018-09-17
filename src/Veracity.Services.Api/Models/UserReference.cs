using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class UserReference : ItemReference
    {
        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        public override string Identity =>string.IsNullOrWhiteSpace(Id)?null: $"{MyExtensions.ServiceRootModifier}/users/{Id}";

        [JsonProperty("email")]
        public override string Description { get; set; }
    }

    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class AdminReference : UserReference
    {
        [JsonProperty("accessLevelUrl")]
        public  string AccessLevelUrl => string.IsNullOrWhiteSpace(ServiceId) ?$"/this/administrators/{Id}": $"/this/administrators/{ServiceId}/{Id}";

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }
    }

    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class UserCreationReference : UserReference
    {
        /// <summary>
        /// The url to include in the email sent to the user to complete the registration process. Only contains a value if the sendEmail flag is false.
        /// </summary>
        [JsonProperty("confirmationUrl")]
        public string ConfirmationUrl { get; set; }
    }
}
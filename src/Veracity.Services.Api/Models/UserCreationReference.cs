using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
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
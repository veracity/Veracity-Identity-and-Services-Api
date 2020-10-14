using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class RegistrationOptions
    {
        /// <summary>
        /// Set this to false to take responsibillity of sending the registration email to the user.
        /// </summary>
        /// <value>
        /// The send mail.
        /// </value>
        [JsonProperty("sendMail")]
        public bool? SendMail { get; set; }

        /// <summary>
        /// Make the service create a default subscription for the newly created user
        /// </summary>
        /// <value>
        /// The create subscription.
        /// </value>
        [JsonProperty("createSubscription")]
        public bool? CreateSubscription { get; set; }

        /// <summary>
        /// The service id to create subscription for
        /// </summary>
        /// <value>
        /// The service identifier.
        /// </value>
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        /// <summary>
        /// Specify the accessLevel/role the user should have with the new subscription. Optional
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        [JsonProperty("role")]
        public string Role { get; set; }


        [JsonProperty("subscriptions")]
        public SubscriptionRequest[] Subscriptions { get; set; }

        /// <summary>
        /// Specify the location to send the newly created user to after the registration process is completed
        /// </summary>
        [JsonProperty("returnUrl")]
        public string returnUrl { get; set; }

	    /// <summary>
	    /// The email address of the user or service that creates the new user account
	    /// </summary>
	    [JsonProperty("contactEmail")]
	    public string ContactEmail { get; set; }


	    /// <summary>
	    /// The Name of the user or service that creates the new user account
	    /// </summary>
	    [JsonProperty("contactName")]
	    public string ContactName { get; set; }

        [JsonProperty("inviteMessage")]
        public string InviteMessage { get; set; }
    }

    public class SubscriptionRequest
    {
        /// <summary>
        /// Mandatory, The service id to create the subscription for
        /// </summary>
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        /// <summary>
        /// Optional, the access level 
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// Optional, define the sku if applicable
        /// </summary>
        [JsonProperty("sku")]
        public string Sku { get; set; }
    }
}
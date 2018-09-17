using System;
using Newtonsoft.Json;
using Stardust.Particles;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    public class UserInfo
    {
        private string _id;

        /// <summary>
        /// Contains the users formatted name: {lastName}, {firstName}. the id token contains the discrete values in the givenName and surname claims.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The users registered email address. if verifiedEmail is true this can be used to contact the user.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("id")]
        public string Id
        {
            get => _id?.ToLowerInvariant();
            set => _id = value?.ToLowerInvariant();
        }

        /// <summary>
        /// Contains the default company affiliation if any.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [Obsolete("This will be removed at a later stage. This has little value as users may have more than one company. Use 'my/companies' to get the full list")]
        [JsonProperty("company")]
        public CompanyReference Company { get; set; }

        [JsonProperty("#companies")]
        public int NumberOfCompanies { get; set; }

        /// <summary>
        ///  <c>true</c> if email is verified by the user; otherwise, <c>false</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if email is verified by the user; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("verifiedEmail")]
        public bool VerifiedEmail { get; set; }

        /// <summary>
        /// Contains the perfered language for the user. If your service support multi-language use this.
        /// </summary>
        /// <value>
        /// The language iso code
        /// </value>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        [JsonProperty("identity")]
        public virtual string Identity { get; set; }

        /// <summary>
        /// Gets the relative url to the users service lits
        /// </summary>
        /// <value>
        /// The services URL.
        /// </value>
        [JsonProperty("servicesUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string ServicesUrl => string.IsNullOrWhiteSpace(Id) ? null : $"{MyExtensions.ServiceRootModifier}/users/{Id}/services?page=0&pageSize=10";

        /// <summary>
        /// Gets the relative url to the users companies lits
        /// </summary>
        /// <value>
        /// The companies URL.
        /// </value>
        [JsonProperty("companiesUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string CompaniesUrl => string.IsNullOrWhiteSpace(Id) ? null : $"{MyExtensions.ServiceRootModifier}/users/{Id}/companies";

        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }

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

    public class MyUserInfo : UserInfo
    {
        [JsonProperty("profilePageUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public string ProfilePageUrl => $"{ConfigurationManagerHelper.GetValueOnKey("myDnvgl.portal.root", "https://localhost:52400")}/EditProfile";

        [JsonProperty("messagesUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public string MessagesUrl { get{ return string.IsNullOrWhiteSpace(Id) ? null : $"/my/messages"; } }

        [JsonProperty("identity")]
        public override string Identity { get; set; }

        /// <summary>
        /// Gets the relative url to the users service lits
        /// </summary>
        /// <value>
        /// The services URL.
        /// </value>
        [JsonProperty("servicesUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public override string ServicesUrl => string.IsNullOrWhiteSpace(Id) ? null : $"/my/services?page=0&pageSize=10";

        /// <summary>
        /// Gets the relative url to the users companies lits
        /// </summary>
        /// <value>
        /// The companies URL.
        /// </value>
        [JsonProperty("companiesUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public override string CompaniesUrl => string.IsNullOrWhiteSpace(Id) ? null : $"/my/companies";
    }
}
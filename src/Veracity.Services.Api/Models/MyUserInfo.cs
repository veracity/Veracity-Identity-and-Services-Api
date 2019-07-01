using Newtonsoft.Json;
using Stardust.Particles;

namespace Veracity.Services.Api.Models
{
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
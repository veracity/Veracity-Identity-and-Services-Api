using System;
using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public class CompanyInfo
    {
        private string _id;
        private string _email;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("usersUrl")]
        public string UsersUrl { get; set; }

        [JsonProperty("addressLines")]
        public string[] AddressLines { get; set; }

        [JsonProperty("id")]
        public string Id
        {
            get => _id?.ToLowerInvariant();
            set => _id = value?.ToLowerInvariant();
        }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("zipCode")]
        public string Zip { get; set; }

        [JsonProperty("#employee")]
        public int EmployeeCount { get; set; }

        [JsonProperty("domains")]
        public string Domains { get; set; }

        [JsonProperty("email")]
        public string Email
        {
            get => _email?.ToLowerInvariant();
            set => _email = value?.ToLowerInvariant();
        }

        [JsonProperty("#requests")]
        public int Requests { get; set; }


        [JsonProperty("internalId")]
        [Obsolete("Should only be used by internal applications", false)]
        public string InternalId { get; set; }

        [Obsolete("Should only be used by internal applications", false)]
        public Address[] Adresses { get; set; }
    }
    public class Address
    {
        [JsonProperty("addressType")]
        public string AddressType { get; set; }

        [JsonProperty("addressLines")]
        public string[] AddressLines { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("stateRegion")]
        public string StateRegion { get; set; }

        [JsonProperty("invoiceSameAsVisiting")]
        public bool? InvoiceSameAsVisiting { get; set; }

        [JsonProperty("zipCode")]
        public string Zip { get; set; }
    }
}
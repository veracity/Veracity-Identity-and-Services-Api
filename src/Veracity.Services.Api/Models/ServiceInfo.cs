using System;
using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    public class ServiceInfo
    {
        private string _id;
        private string _parentId;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("apiAudience")]
        public string ApiAudience { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        //
        // Summary:
        //     The pricing tier of the service installed in the tenant, optional.
        public string PricingTier { get; set; }

        [JsonProperty("id")]
        public string Id
        {
            get => _id?.ToLowerInvariant();
            set => _id = value?.ToLowerInvariant();
        }

        [JsonProperty("inherited")]
        public bool Inherited { get; set; }

        [JsonProperty("selfSubscribe")]
        public bool SelfSubscribe { get; set; }

        [JsonProperty("serviceOwner")]
        public string ServiceOwner { get; set; }

        [JsonProperty("termsOfUse")]
        public string TermsOfUse { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTime? LastUpdated { get; set; }

        [JsonProperty("parentUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public string ParentUrl => string.IsNullOrWhiteSpace(ParentId) ? null : $"{MyExtensions.ServiceRootModifier}/services({ParentId})";

        [JsonProperty("parentId")]
        public string ParentId
        {
            get { return _parentId?.ToLowerInvariant(); }
            set { _parentId = value?.ToLowerInvariant(); }
        }


        [JsonProperty("childrenUrl", DefaultValueHandling = DefaultValueHandling.Include)]
        public string ServicesUrl => $"{MyExtensions.ServiceRootModifier}/services/{Id}/services";

        [JsonProperty("servicerUrl")]
        public string ServiceUrl { get; set; }
    }
}
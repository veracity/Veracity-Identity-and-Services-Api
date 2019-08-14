using System;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class CompanyReference : ItemReference
    {
        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        public override string Identity => $"{MyExtensions.ServiceRootModifier}/companies/{Id}";

        [Obsolete]
        public string InternalId { get; set; }

        [Obsolete]
        public bool? IsAdmin { get; set; }

        //Make the .net sdk graph like
        //[JsonIgnore]
        //public Task<CompanyInfo> CompanyDetailsAsync => MyExtensions.Companies().CompanyById(Id);

        //[JsonIgnore]
        //public CompanyInfo CompanyDetails => Task.Run(async () => await CompanyDetailsAsync).Result;
    }
}
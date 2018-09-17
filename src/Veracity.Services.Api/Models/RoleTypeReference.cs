using System;
using Newtonsoft.Json;
using Veracity.Services.Api.Extensions;

namespace Veracity.Services.Api.Models
{
    /// <summary>
    /// Contians the name, id and relative uri to the resource detilas
    /// </summary>
    /// <seealso cref="ItemReference" />
    public class RoleTypeReference : ItemReference
    {
        /// <summary>
        /// The relative path to the resource details
        /// </summary>
        /// <value>
        /// The relative path to the resource details
        /// </value>
        public override string Identity => $"{MyExtensions.ServiceRootModifier}/roles/{Id}";

        [JsonIgnore]
        [Obsolete]
        public override string Description { get; set; }
    }
}
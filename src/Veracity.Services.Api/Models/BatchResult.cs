using System.Collections.Generic;

namespace Veracity.Services.Api.Models
{
    public class BatchResult<T>
    {

        public List<T> Results { get; set; }
        public Dictionary<string, string> Failures { get; set; }
    }
}
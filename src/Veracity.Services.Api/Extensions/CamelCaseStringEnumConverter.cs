using Newtonsoft.Json.Converters;

namespace Veracity.Services.Api.Extensions
{
    public sealed class CamelCaseStringEnumConverter : StringEnumConverter
    {
        public CamelCaseStringEnumConverter() : base(true)
        {

        }
    }
}
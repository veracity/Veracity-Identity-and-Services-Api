using Newtonsoft.Json;

namespace Veracity.Services.Api.Models
{
    public enum SubscriptionStateTypes
    {
        Subscribing = 0,
        Pending = 1,
        Rejected = 2,
        NotSubscribing = 3,
        NotDefined = 4,
        Error = 5
    }
}
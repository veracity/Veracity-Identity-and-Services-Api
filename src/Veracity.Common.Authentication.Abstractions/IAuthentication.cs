using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Veracity.Common.Authentication
{
    public interface IAuthentication
    {
        Task<HttpWebRequest> ApplyAuthenticationHeaderAsync(HttpWebRequest request);
    }
    public interface IUserNameResolver
    {
        string GetCurrentUserName();

        string GetActorId();
        ClaimsPrincipal User { get; }
        string UserId { get; }
    }
}
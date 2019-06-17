using System.Security.Claims;

namespace Veracity.Common.Authentication
{
    public interface IUserNameResolver
    {
        string GetCurrentUserName();

        string GetActorId();
        ClaimsPrincipal User { get; }
        string UserId { get; }
    }
}
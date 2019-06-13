using System.Threading.Tasks;

namespace Veracity.Common.Authentication
{
    public interface ITokenHandler
    {
        Task<string> GetBearerTokenAsync();
    }
}
using System.Net;
using System.Threading.Tasks;

namespace Veracity.Common.Authentication
{
    public interface IAuthentication
    {
        Task<HttpWebRequest> ApplyAuthenticationHeaderAsync(HttpWebRequest request);
    }
}
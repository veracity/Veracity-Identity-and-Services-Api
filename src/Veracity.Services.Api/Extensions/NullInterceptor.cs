using System.Net;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Extensions;

namespace Veracity.Services.Api.Extensions
{
    public class NullInterceptor : IInputInterceptor
    {
        public object Intercept(object result, StateDictionary getState)
        {
            return result;
        }

        public bool Intercept(object[] values, StateDictionary stateDictionary, out string cancellationMessage,
            out HttpStatusCode statusCode)
        {
            cancellationMessage = null;
            statusCode = HttpStatusCode.OK;
            return false;
        }

        public Task<object> InterceptAsync(object result, StateDictionary getState)
        {
            return Task.FromResult(result);
        }

        public Task<InterseptorStatus> InterceptAsync(object[] values, StateDictionary stateDictionary)
        {
            return Task.FromResult(new InterseptorStatus { Cancel = false });
        }
    }
}
using System;
using System.Net;
using System.Threading.Tasks;

namespace Veracity.Common.Authentication.Abstractions
{
	public interface ITokenHandler
	{
		Task<string> GetBearerTokenAsync();
	}

	public interface IAuthentication
	{
		Task<HttpWebRequest> ApplyAuthenticationHeaderAsync(HttpWebRequest request);
	}
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stardust.Interstellar.Rest.Client;
using Veracity.Services.Api;

namespace HelloAspNetCore22.Pages
{
	[Authorize]
	public class IndexModel : PageModel
	{
        private readonly IApiClient _apiClient;

        public IndexModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }
		public async Task OnGet()
        {
            var isDnvglUser = false;
            var userProfile = await _apiClient.My.AddHeaderValue("x-include-internal-identity","true").Info();
            if (userProfile.Extensions.TryGetValue("domain", out var domain) && domain == "VERIT") isDnvglUser = true;
            ViewData.Add("userProfile",userProfile);
        }
	}
}

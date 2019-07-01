using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            var userProfile = await _apiClient.My.Info();
            ViewData.Add("userProfile",userProfile);
        }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stardust.Interstellar.Rest.Client;
using Stardust.Particles;
using Veracity.Services.Api;

namespace HelloAspNetCore.Pages
{
    //[Authorize(Policy = "AzureAdB2C")]
    //[Authorize(Policy = "OAuth")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }


        public async Task OnGet()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _api.My.SetSupportCode(User).Info();
                    ViewData["UserName"] = user.Name;
                }
                else
                {
                    ViewData["UserName"] = null;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}

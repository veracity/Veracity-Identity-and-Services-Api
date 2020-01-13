using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Veracity.Services.Api;

namespace HelloAspNetCore31.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _client;

        public IndexModel(IApiClient client)
        {
            _client = client;
        }
        public async Task OnGet()
        {
            UserName = (await _client.My.Info()).Name;
        }

        public string UserName { get; set; }
    }
}

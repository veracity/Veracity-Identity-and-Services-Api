using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Veracity.Common.Authentication;

namespace VeracityIdentity.Pages
{
    [Authorize]
    public class IndexModel : PageModel,IDisposable
    {
        private readonly ITokenHandler _tokenHandler;
        private readonly TokenProviderConfiguration _config;
        private HttpClient _client;

        public IndexModel(ITokenHandler tokenHandler,TokenProviderConfiguration config)
        {
            _tokenHandler = tokenHandler;
            _config = config;
            _client = new HttpClient(new CustomHttpHandler(config,tokenHandler));
        }
        public async Task OnGet()
        {
            var userdata = await _client.GetStringAsync($"{_config.MyServicesApi}/my/profile");
            ViewData.Add("user",userdata);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class CustomHttpHandler : HttpClientHandler
    {
        private readonly TokenProviderConfiguration _config;
        private readonly ITokenHandler _handler;

        public CustomHttpHandler(TokenProviderConfiguration config,ITokenHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            request.Headers.Authorization=AuthenticationHeaderValue.Parse(await _handler.GetBearerTokenAsync(_config.Scope));
            request.Headers.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

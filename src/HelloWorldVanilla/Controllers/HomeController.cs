using Microsoft.Ajax.Utilities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api;
using Veracity.Services.Api.Models;

namespace HelloWorldVanilla.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiClient _veracityClient;

        public HomeController(IApiClient veracityClient)
        {
            _veracityClient = veracityClient;
        }
        public async Task<ActionResult> Index()
        {
            var user = new UserInfo();
            ViewBag.Title = "Home Page";
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
                user = await _veracityClient.My.Info();
            }
            return View(user);
        }

        public void Login(string redirectUrl)
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge();
                return;
            }
            if (redirectUrl.IsNullOrWhiteSpace()) redirectUrl = "/";

            Response.Redirect(redirectUrl);
        }


        public void Logout(string redirectUrl)
        {
            Response.Logout(redirectUrl);
        }
    }
}

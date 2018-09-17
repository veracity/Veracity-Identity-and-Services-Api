using Microsoft.Ajax.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Veracity.Common.OAuth.Providers;
using Veracity.Services.Api;
using Veracity.Services.Api.Models;

namespace HelloWorld.Controllers
{

    public class HomeController : Controller
    {
        private readonly IApiClient _myServicesClient;
        public HomeController(IApiClient myServicesClient)
        {
            _myServicesClient = myServicesClient;
        }
        /// <summary>
        /// retreives the users profile using the SDK
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var user = new UserInfo();
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
                user = await _myServicesClient.My.Info();
            }
            return View(user);
        }

        /// <summary>
        /// retreives the users companies using a basic http client
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> About()
        {
            ViewBag.Message = "Getting your companies with HttpClient";
            if (Request.IsAuthenticated)
            {

                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
                var client = new HttpClient
                {
                    BaseAddress = new Uri(ConfigurationManager.AppSettings["myApiV3Url"]),
                    DefaultRequestHeaders =
                    {
                        Authorization = AuthenticationHeaderValue.Parse(new TokenProvider().GetBearerToken())

                    }
                };
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["subscriptionKey"]);
                var companies = await client.GetAsync("my/companies");

                ViewBag.CompaniesRawData = await companies.Content.ReadAsStringAsync();

            }
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }
            return View();
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
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
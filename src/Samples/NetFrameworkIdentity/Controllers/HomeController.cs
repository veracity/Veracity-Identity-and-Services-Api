using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace NetFrameworkIdentity.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }

            return View();
        }

        public ActionResult About()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
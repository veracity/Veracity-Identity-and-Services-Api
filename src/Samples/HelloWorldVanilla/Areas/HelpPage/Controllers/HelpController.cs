using HelloWorldVanilla.Areas.HelpPage.ModelDescriptions;
using HelloWorldVanilla.Areas.HelpPage.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Mvc;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace HelloWorldVanilla.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        private const string ErrorViewName = "Error";

        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        public HttpConfiguration Configuration { get; private set; }

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }

            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        public ActionResult Api(string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                if (Request.IsAuthenticated)
                {
                    ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
                }
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View(ErrorViewName);
        }

        public ActionResult ResourceModel(string modelName)
        {
            if (Request.IsAuthenticated)
            {
                ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            }
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = Configuration.GetModelDescriptionGenerator();
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out ModelDescription modelDescription))
                {
                    return View(modelDescription);
                }
            }

            return View(ErrorViewName);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Veracity.Services.Api;

namespace HelloAspNetCore31.Controllers
{
    public class TestController : Controller
    {
        private readonly IMy _myService;

        public TestController(IMy myService)
        {
            _myService = myService;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _myService.Info());
        }
    }
}

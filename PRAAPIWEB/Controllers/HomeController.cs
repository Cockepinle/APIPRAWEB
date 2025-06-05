using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRAAPIWEB.Models;
using System.Diagnostics;

namespace PRAAPIWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error(string message, string returnUrl = null)
        {
            return View(new ErrorViewModel
            {
                ErrorMessage = message,
                ReturnUrl = returnUrl ?? Url.Action("Index", "Home"),
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

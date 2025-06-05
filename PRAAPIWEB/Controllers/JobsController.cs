using Microsoft.AspNetCore.Mvc;

namespace PRAAPIWEB.Controllers
{
    public class JobsController : Controller
    {
        public IActionResult JobSearch()
        {
            return View();
        }
    }
}

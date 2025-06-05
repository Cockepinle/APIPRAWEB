using Microsoft.AspNetCore.Mvc;

namespace PRAAPIWEB.Controllers
{
    public class MapController : Controller
    {
        public IActionResult HelpMap()
        {
            return View();
        }
    }
}

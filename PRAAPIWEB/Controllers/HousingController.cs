using Microsoft.AspNetCore.Mvc;

namespace PRAAPIWEB.Controllers
{
    public class HousingController : Controller
    {
        public IActionResult Accommodation()
        {
            return View();
        }
    }
}

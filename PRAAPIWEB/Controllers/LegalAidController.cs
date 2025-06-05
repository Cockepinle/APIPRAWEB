using Microsoft.AspNetCore.Mvc;

namespace PRAAPIWEB.Controllers
{
    public class LegalAidController : Controller
    {
        public IActionResult LegalHelp()
        {
            return View();
        }
    }
}

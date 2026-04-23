using Microsoft.AspNetCore.Mvc;

namespace PROGPOE.Controllers
{
    public class MvcAccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace PROGPOE.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [Route("Account/Login")]
        public IActionResult Login()
        {
            return View();
        }
    }
}
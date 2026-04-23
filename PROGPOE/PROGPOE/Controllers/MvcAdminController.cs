using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PROGPOE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult Contracts() => View();
        public IActionResult ServiceRequests() => View();
    }
}
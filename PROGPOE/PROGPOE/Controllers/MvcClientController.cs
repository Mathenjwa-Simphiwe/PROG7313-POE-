using Microsoft.AspNetCore.Mvc;

namespace PROGPOE.Controllers
{
    // MVC controller that serves Razor Views for the Client area.
    // Authorization is handled client-side via JWT stored in localStorage.
    // [Authorize] is intentionally omitted: the app uses JWT Bearer tokens
    // (not cookies), so server-side [Authorize] blocks the page load before
    // the browser can read localStorage and redirect appropriately.
    public class ClientController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult ClientContracts() => View();
        public IActionResult Requests() => View();
    }
}

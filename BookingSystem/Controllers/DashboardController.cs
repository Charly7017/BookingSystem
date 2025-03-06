using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

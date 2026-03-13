using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

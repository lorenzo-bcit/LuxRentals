using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers.TestControllers
{
    //TODO: Remove this Controller later.
    public class CarController : Controller
    {
        // GET: Car/Shop (Simple shop page for testing)
        public IActionResult Shop()
        {
            return View();
        }
    }
}
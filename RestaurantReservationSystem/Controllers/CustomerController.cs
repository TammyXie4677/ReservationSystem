using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantReservationSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        public IActionResult CustomerDashboard()
        {
            return View();
        }
    }
}

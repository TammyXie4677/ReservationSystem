using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;

namespace RestaurantReservationSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingController : Controller
    {
        private readonly RestaurantReservationContext _context;

        public BookingController(RestaurantReservationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Reserve(int id)
        {
            // Retrieve restaurant details directly from the database.
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
            {
                // Handle case where the restaurant doesn't exist.
                return NotFound();
            }

            // Pass restaurant details to the view.
            ViewBag.RestaurantId = restaurant.RestaurantId;
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantAddress = restaurant.Address;
            ViewBag.RestaurantPhone = restaurant.PhoneNumber;
            ViewBag.RestaurantEmail = restaurant.Email;

            return View("Create");
        }
    }
}

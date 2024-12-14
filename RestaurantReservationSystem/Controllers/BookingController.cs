using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
using System.Linq;
using System.Security.Claims;

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
            // Retrieve the restaurant details from the database
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
            {
                return NotFound(); // Handle case where restaurant does not exist
            }

            // Retrieve the logged-in user's details from claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound(); // Handle case where the user does not exist
            }

            // Pass restaurant and user details to the view via ViewBag
            ViewBag.RestaurantId = restaurant.RestaurantId;
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantAddress = restaurant.Address;
            ViewBag.RestaurantPhone = restaurant.PhoneNumber;
            ViewBag.RestaurantEmail = restaurant.Email;

            // Pass the logged-in user's information to the form fields
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.Email = user.Email;

            return View("Create");
        }

        // Action for Return
        public IActionResult Return()
        {
            // You can choose to redirect the user to another view, like the restaurant list or the home page
            return RedirectToAction("CustomerDashboard", "Customer");
        }
    }
}


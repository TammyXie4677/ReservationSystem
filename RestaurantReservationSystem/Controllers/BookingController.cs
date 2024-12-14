using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
using System;
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
            var restaurant = _context.Restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
            {
                return NotFound(); // Handle case where restaurant does not exist
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound(); // Handle case where user does not exist
            }

            ViewBag.RestaurantId = restaurant.RestaurantId;
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantAddress = restaurant.Address;
            ViewBag.RestaurantPhone = restaurant.PhoneNumber;
            ViewBag.RestaurantEmail = restaurant.Email;

            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.Email = user.Email;

            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int RestaurantId, string FirstName, string LastName, string PhoneNumber, string Email, 
                                    int Guests, string ReservationDate, string ReservationTime)
        {
            if (!DateTime.TryParse(ReservationDate, out var datePart))
            {
                ModelState.AddModelError("ReservationDate", "Invalid reservation date.");
            }

            if (!TimeSpan.TryParse(ReservationTime, out var timePart))
            {
                ModelState.AddModelError("ReservationTime", "Invalid reservation time.");
            }

            if (!ModelState.IsValid)
            {
                return View("Create");
            }

            // Combine date and time
            var bookingDate = datePart.Add(timePart);

            // Retrieve the logged-in user's details
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound(); // Handle case where user does not exist
            }

            // Create a new booking object
            var booking = new Booking
            {
                UserId = user.UserId,
                RestaurantId = RestaurantId,
                BookingDate = bookingDate,
                GuestsCount = Guests,
                Status = 1, // Assuming 1 represents a "confirmed" booking status
                CreatedAt = DateTime.Now
            };

            // Add the booking to the database
            _context.Bookings.Add(booking);
            _context.SaveChanges();
            
            return RedirectToAction("CustomerDashboard", "Customer");
        }

        public IActionResult Return()
        {
            return RedirectToAction("CustomerDashboard", "Customer");
        }
    }
}

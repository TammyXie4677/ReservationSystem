using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.ViewModels;
using System;
using System.Globalization;
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
            var restaurant = _context.Restaurants.Find(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantAddress = restaurant.Address;
            ViewBag.RestaurantPhone = restaurant.PhoneNumber;
            ViewBag.RestaurantEmail = restaurant.Email;

            var model = new BookingViewModel
            {
                RestaurantId = restaurant.RestaurantId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Guests = 1
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookingViewModel model)
        {
            // Validate ReservationDate and ReservationTime
            if (!DateTime.TryParseExact(model.ReservationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datePart))
            {
                ModelState.AddModelError("ReservationDate", "Invalid reservation date.");
            }

            if (!TimeSpan.TryParseExact(model.ReservationTime, @"hh\:mm", CultureInfo.InvariantCulture, out var timePart))
            {
                ModelState.AddModelError("ReservationTime", "Invalid reservation time.");
            }

            // Validate number of guests
            if (model.Guests < 1 || model.Guests > 20)
            {
                ModelState.AddModelError("Guests", "Number of guests must be between 1 and 20.");
            }

            // If model state is invalid, return with errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    // Log or inspect the error
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(model);
            }

            // Combine the parsed date and time into a single DateTime (BookingDate)
            var bookingDate = datePart.Add(timePart);

            // Find the current user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            // ViewBag to pass updated model to the view
            ViewBag.UpdatedBookingData = new BookingViewModel
            {
                RestaurantId = model.RestaurantId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Guests = model.Guests,
                ReservationDate = model.ReservationDate,
                ReservationTime = model.ReservationTime
            };

            // Create the booking object
            var booking = new Booking
            {
                UserId = user.UserId,
                RestaurantId = model.RestaurantId,
                BookingDate = bookingDate,
                GuestsCount = model.Guests,
                Status = 1, // Assuming 1 represents a "confirmed" booking status
                CreatedAt = DateTime.Now
            };

            // Save the booking to the database
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Redirect to the ReservationDetails page
            return RedirectToAction("ReservationDetails", "Booking");
        }


        public IActionResult Return()
        {
            return RedirectToAction("CustomerDashboard", "Customer");
        }

        [HttpGet]
        public IActionResult ReservationDetails()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var reservations = _context.Bookings
                .Where(b => b.UserId == user.UserId)
                .Include(b => b.Restaurant)
                .OrderBy(b => b.BookingDate)
                .ToList();

            return View(reservations);
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Restaurant)
                .SingleOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null || user.UserId != booking.UserId)
            {
                return Forbid();
            }

            var model = new BookingViewModel
            {
                BookingId = booking.BookingId,
                RestaurantId = booking.RestaurantId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Guests = booking.GuestsCount,
                Status = booking.Status,
                ReservationDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                ReservationTime = booking.BookingDate.ToString("HH:mm")
            };

            ViewBag.RestaurantName = booking.Restaurant.Name;
            ViewBag.RestaurantAddress = booking.Restaurant.Address;
            ViewBag.RestaurantPhone = booking.Restaurant.PhoneNumber;
            ViewBag.RestaurantEmail = booking.Restaurant.Email;

            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookingViewModel model)
        {
            if (!DateTime.TryParseExact(model.ReservationDate + " " + model.ReservationTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var bookingDate))
            {
                ModelState.AddModelError("ReservationDate", "Invalid reservation date or time.");
            }

            if (model.Guests < 1 || model.Guests > 20)
            {
                ModelState.AddModelError("Guests", "Number of guests must be between 1 and 20.");
            }

            if (model.Status != 1 && model.Status != 0)
            {
                ModelState.AddModelError("Status", "Invalid reservation status.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var booking = _context.Bookings.SingleOrDefault(b => b.BookingId == model.BookingId);

            if (booking == null)
            {
                return NotFound();
            }

            booking.GuestsCount = model.Guests;
            booking.BookingDate = bookingDate;
            booking.Status = model.Status;
            _context.SaveChanges();

            return RedirectToAction("ReservationDetails");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings.SingleOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }

            // Ensure the user is authorized to delete this booking
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null || user.UserId != booking.UserId)
            {
                return Json(new { success = false, message = "You are not authorized to delete this booking." });
            }

            // Delete the booking
            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}

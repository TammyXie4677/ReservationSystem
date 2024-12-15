using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;

namespace RestaurantReservationSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBookingController : Controller
{
    private readonly RestaurantReservationContext _context;

    public AdminBookingController(RestaurantReservationContext context)
    {
        _context = context;
    }

    // view all bookings + filter and search functionality
    [HttpGet]
    public IActionResult Index(string? Name, DateTime? BookingDate)
    {
        var bookings = _context.Bookings
            .Include(b => b.Restaurant)
            .Include(b => b.User)
            .AsQueryable();

        // filter restaurant name
        if (!string.IsNullOrEmpty(Name) && Name.Length >= 2)
        {
            bookings = bookings.Where(b => b.Restaurant.Name.Contains(Name));
        }

        // filter booking date
        if (BookingDate.HasValue)
        {
            bookings = bookings.Where(b => b.BookingDate.Date == BookingDate.Value.Date);
        }

        return View(bookings.ToList());
    }

    // API for restaurant name auto-complete
        [HttpGet]
        public JsonResult GetRestaurantNames(string term)
        {
            // Minimum input length check
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<string>());
            }

            var restaurantNames = _context.Restaurants
                .Where(r => r.Name.Contains(term)) // Use StartsWith for prefix matching
                .Select(r => new { Name = r.Name, Address = r.Address })
                .ToList();

            return Json(restaurantNames);
        }
    
    // create booking - show form
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Restaurants = _context.Restaurants.ToList();
        return View();
    }
    
    // create booking - submit form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Booking booking)
    {
        if (ModelState.IsValid)
        {
            booking.CreatedAt = DateTime.Now;
            _context.Bookings.Add(booking);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Restaurants = _context.Restaurants.ToList();
        return View(booking);
    }

    // edit booking - show form
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var booking = _context.Bookings.Include(b => b.Restaurant).FirstOrDefault(b => b.BookingId == id);
        if (booking == null)
        {
            return NotFound();
        }

        ViewBag.Restaurants = _context.Restaurants.ToList();
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Booking booking)
    {
        if (ModelState.IsValid)
        {
            booking.UpdatedAt = DateTime.Now;
            _context.Bookings.Update(booking);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Restaurants = _context.Restaurants.ToList();
        return View(booking);
    }

    // delete booking
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var booking = _context.Bookings.Find(id);
        if (booking == null)
        {
            return NotFound();
        }

        _context.Bookings.Remove(booking);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}

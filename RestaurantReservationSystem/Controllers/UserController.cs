using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantReservationSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly RestaurantReservationContext _context;

        public UserController(RestaurantReservationContext context)
        {
            _context = context;
        }

        // GET: Registration form
        public IActionResult Register()
        {
            return View();
        }

        // POST: Handle registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the email already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View(model);
            }

            // Hash the password
            var hashedPassword = HashPassword(model.Password);

            // Create a new user
            var user = new User
            {
                Email = model.Email,
                PasswordHash = hashedPassword,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserRole = "Customer",
                CreatedAt = DateTime.Now
            };

            // Save to the database
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login", "User");
        }

        // Password hashing helper
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

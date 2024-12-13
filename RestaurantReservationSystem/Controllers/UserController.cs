using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.ViewModels;
using System.Security.Claims;
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

        // GET: Login form
        public IActionResult Login()
        {
            return View();
        }

        // POST: Handle login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Hash the entered password
            var hashedPassword = HashPassword(model.Password);

            // Validate user credentials
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRole)
            };

            // Create the authentication cookie
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember user across browser sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect to home or dashboard
            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

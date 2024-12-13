using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        // GET: Manage Profile
        [Authorize(Roles = "Customer")]
        public IActionResult Profile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // POST: Handle Profile Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public IActionResult Profile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            // Check if the new email is already registered (excluding the current user's email)
            if (_context.Users.Any(u => u.Email == model.Email && u.Email != user.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Update user information (Only update fields that are provided)
            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Email = model.Email ?? user.Email;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

            // Only update the password if it's provided and confirmed
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Validate password confirmation if a password is provided
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(model);
                }

                // Update the password hash if it's provided and confirmed
                user.PasswordHash = HashPassword(model.Password);
            }

            // Ensure the entity is being tracked before saving
            _context.Users.Attach(user);

            _context.SaveChanges();

            // Set a success message using TempData
            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToAction("Profile"); // Redirect to the same page after the update
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
                UserRole = "Customer", // Default role for registered users
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
                new Claim(ClaimTypes.Role, user.UserRole) // Assign the user's role
            };

            // Create the authentication cookie
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember user across browser sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect based on user role
            if (user.UserRole == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else if (user.UserRole == "Customer")
            {
                return RedirectToAction("CustomerDashboard", "Customer");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }

        // GET: Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // Create an AccessDenied view to display an error message
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

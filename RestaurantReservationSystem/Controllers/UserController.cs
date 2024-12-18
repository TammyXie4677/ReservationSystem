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
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            // Check initial model state
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

            // Explicitly check for email uniqueness
            if (model.Email != null && model.Email.ToLower() != user.Email.ToLower())
            {
                // Check if the new email already exists in the database for ANY OTHER user
                bool emailExists = _context.Users
                    .Any(u => u.Email.ToLower() == model.Email.ToLower() && u.UserId != user.UserId);

                if (emailExists)
                {
                    // Add model error to prevent form submission
                    ModelState.AddModelError("Email", "This email is already registered by another user.");
                    return View(model);
                }
            }

            // Validate password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(model);
                }

                user.PasswordHash = HashPassword(model.Password);
            }

            // Update user information only for fields that are provided
            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Email = model.Email ?? user.Email;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

            // Recreate authentication cookie with updated name
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.UserRole)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToAction("Profile");
        }


        // GET: Registration form
        public IActionResult Register()
        {
            return View();
        }

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
                ModelState.AddModelError("Email", "Email is already registered.");
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
        // DELETE: Delete Account
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get the logged-in user's email
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            // Remove associated bookings (if any)
            var userBookings = _context.Bookings.Where(b => b.UserId == user.UserId).ToList();
            if (userBookings.Any())
            {
                _context.Bookings.RemoveRange(userBookings);
            }

            // Remove the user from the database
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Log the user out after account deletion
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Display a success message
            TempData["SuccessMessage"] = "Your account has been deleted successfully.";
            return RedirectToAction("Index", "Home");
        }
    }



}

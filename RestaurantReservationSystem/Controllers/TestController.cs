using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;  // Adjust namespace as needed
using System.Linq;

namespace RestaurantReservationSystem.Controllers
{
    public class TestController : Controller
    {
        private readonly RestaurantReservationContext _context;

        // Inject the DbContext into the controller
        public TestController(RestaurantReservationContext context)
        {
            _context = context;
        }

        // Action to test database connection
        public IActionResult Index()
        {
            // Try to retrieve some data from one of the tables (e.g., User table)
            var users = _context.Users.ToList();

            if (users != null && users.Any())
            {
                // If data is returned, the connection is successful
                return Content("Database connection is successful! Found " + users.Count + " users.");
            }
            else
            {
                // If no data is returned, check if the table is empty or the connection failed
                return Content("Database connection is successful, but no users found.");
            }
        }
    }
}

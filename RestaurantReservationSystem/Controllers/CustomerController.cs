using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.ViewModels;
using System.Linq;

namespace RestaurantReservationSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly RestaurantReservationContext _context;

        public CustomerController(RestaurantReservationContext context)
        {
            _context = context;
        }

        public IActionResult CustomerDashboard()
        {
            // Fetch the list of available restaurants from the database
            var restaurants = _context.Restaurants
                .Select(r => new RestaurantViewModel
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    CuisineType = r.CuisineType,
                    PriceRange = r.PriceRange
                })
                .ToList();

            // Pass the list of restaurants to the view
            return View(restaurants);
        }
    }
}

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

        public IActionResult CustomerDashboard(string search)
        {
            // Create a base query to fetch restaurants
            var query = _context.Restaurants.AsQueryable();

            // If there is a search term, filter the restaurants
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.Contains(search) ||
                                          r.CuisineType.Contains(search) ||
                                          r.PriceRange.Contains(search));
                
                // Store the search term for view to prefill the input box
                ViewData["SearchTerm"] = search;
            }

            // Map the results to RestaurantViewModel
            var restaurants = query
                .Select(r => new RestaurantViewModel
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    CuisineType = r.CuisineType,
                    PriceRange = r.PriceRange
                })
                .ToList();

            // Pass the filtered list of restaurants to the view
            return View(restaurants);
        }
    }
}

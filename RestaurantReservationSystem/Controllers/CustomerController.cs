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

            if (!string.IsNullOrEmpty(search))
            {
                // Split the search term into words and remove empty values or extra spaces
                var searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(s => s.ToLower())  // Convert each term to lowercase
                                         .ToList();

                if (searchTerms.Any())  // Only filter if there are valid search terms
                {
                    query = query.Where(r => searchTerms.All(term =>
                        r.Name.ToLower().Contains(term) ||
                        r.CuisineType.ToLower().Contains(term) ||
                        r.PriceRange.ToLower().Contains(term)
                    ));
                }

                // Store the search term for the view to prefill the input box
                ViewData["SearchTerm"] = search;
            }
            else
            {
                // If the search term is empty, set empty search for the input box
                ViewData["SearchTerm"] = "";
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

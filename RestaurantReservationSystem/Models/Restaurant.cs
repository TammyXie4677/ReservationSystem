using System;
using System.Collections.Generic;

namespace YourNamespace.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; } // Primary Key
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CuisineType { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty; // $, $$, $$$
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<Booking>? Bookings { get; set; } // A restaurant can have many bookings
    }
}

using System;

namespace YourNamespace.Models
{
    public class Booking
    {
        public int BookingId { get; set; } // Primary Key
        public int UserId { get; set; } // Foreign Key to User table
        public int RestaurantId { get; set; } // Foreign Key to Restaurant table
        public DateTime BookingDate { get; set; }
        public int GuestsCount { get; set; }
        public int Status { get; set; } // 0 = Pending, 1 = Confirmed, 2 = Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; } // Booking belongs to a User
        public Restaurant? Restaurant { get; set; } // Booking is associated with a Restaurant
    }
}

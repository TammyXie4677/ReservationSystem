using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.ViewModels
{
    public class BookingViewModel
    {
        // RestaurantId to store the restaurant's ID for booking purposes
        public int RestaurantId { get; set; }
        public int BookingId { get; set; }

        // User details filled from the logged-in user
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Phone number must be in the format xxx-xxx-xxxx.")]
        public string PhoneNumber { get; set; } = null!;

        // Date and time for the reservation
        [Required(ErrorMessage = "Reservation Date is required.")]
        public string ReservationDate { get; set; } = null!; // This will be a string in the view for date picker

        [Required(ErrorMessage = "Reservation Time is required.")]
        public string ReservationTime { get; set; } = null!; // This will be a string for the time selection

        // Number of guests for the reservation
        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20.")]
        public int Guests { get; set; }

        [Range(0, 1, ErrorMessage = "Invalid reservation status.")]
        public int Status { get; set; }
    }
}

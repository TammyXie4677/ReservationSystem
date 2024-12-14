using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        // Phone number is required with format validation
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Phone number must be in the format (xxx) xxx-xxxx.")]
        public string? PhoneNumber { get; set; }

        // BookingDate is required
        [Required(ErrorMessage = "Booking Date is required.")]
        public DateTime BookingDate { get; set; }

        // GuestsCount is required with a range
        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20.")]
        public int GuestsCount { get; set; }
    }
}

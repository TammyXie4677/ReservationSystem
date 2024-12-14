using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int RestaurantId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime BookingDate { get; set; }

        [Range(1, 20)] 
        public int GuestsCount { get; set; }
    }
}

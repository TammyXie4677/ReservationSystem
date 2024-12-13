namespace RestaurantReservationSystem.ViewModels
{
    public class RestaurantViewModel
    {
        public int RestaurantId { get; set; }
        public required string Name { get; set; }
        public string? CuisineType { get; set; }
        public required string PriceRange { get; set; }
    }
}

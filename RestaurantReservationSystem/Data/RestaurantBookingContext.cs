using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

public class RestaurantBookingContext : DbContext
{
    public RestaurantBookingContext(DbContextOptions<RestaurantBookingContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Booking> Bookings { get; set; }
}

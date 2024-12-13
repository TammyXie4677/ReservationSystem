using System;
using System.Collections.Generic;

namespace RestaurantReservationSystem.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RestaurantId { get; set; }

    public DateTime BookingDate { get; set; }

    public int GuestsCount { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

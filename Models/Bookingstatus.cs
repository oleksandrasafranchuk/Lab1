using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class BookingStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

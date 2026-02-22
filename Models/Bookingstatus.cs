using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Bookingstatus
{
    public int Id { get; set; }

    public string Statusname { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Bookinghistory> Bookinghistories { get; set; } = new List<Bookinghistory>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

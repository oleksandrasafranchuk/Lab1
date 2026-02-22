using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class User
{
    public int Id { get; set; }

    public int Roleid { get; set; }

    public string Fullname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Bookinghistory> Bookinghistories { get; set; } = new List<Bookinghistory>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Role Role { get; set; } = null!;
}

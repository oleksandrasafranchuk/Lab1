using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Bookinghistory
{
    public long Id { get; set; }

    public int Bookingid { get; set; }

    public int Statustoid { get; set; }

    public int? Changedbyuserid { get; set; }

    public string? Changereason { get; set; }

    public DateTime Changedat { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? Changedbyuser { get; set; }

    public virtual Bookingstatus Statusto { get; set; } = null!;
}

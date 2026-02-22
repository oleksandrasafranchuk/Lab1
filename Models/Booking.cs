using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Workspaceid { get; set; }

    public int Statusid { get; set; }

    public DateTime Starttime { get; set; }

    public DateTime Endtime { get; set; }

    public decimal Totalamount { get; set; }

    public string? Usercomment { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Bookinghistory> Bookinghistories { get; set; } = new List<Bookinghistory>();

    public virtual Bookingstatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Workspace Workspace { get; set; } = null!;
}
